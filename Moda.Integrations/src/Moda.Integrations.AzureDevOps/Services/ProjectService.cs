using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace Moda.Integrations.AzureDevOps.Services;
internal sealed class ProjectService
{
    private readonly ProjectHttpClient _projectClient;
    private readonly ILogger<ProjectService> _logger;
    private readonly int _maxBatchSize = 100;

    public ProjectService(VssConnection connection, ILogger<ProjectService> logger)
    {
        _projectClient = connection.GetClient<ProjectHttpClient>();
        _logger = logger;
    }

    public async Task<Result<List<TeamProjectReference>>> GetProjects()
    {
        try
        {
            List<TeamProjectReference> projects = new();

            while (true)
            {
                var batch = await _projectClient.GetProjects(top: _maxBatchSize, skip: projects.Count);
                projects.AddRange(batch);

                if (batch.Count < _maxBatchSize)
                    break;
            }

            _logger.LogDebug("{ProjectCount} projects found ", projects.Count);

            return Result.Success(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown getting projects from Azure DevOps");
            return Result.Failure<List<TeamProjectReference>>(ex.ToString());
        }
    }

    public async Task<Result<TeamProject>> GetProject(Guid projectId)
    {
        try
        {
            var project = await _projectClient.GetProject(projectId.ToString());
            if (project is null)
            {
                _logger.LogWarning("No project found with id {ProjectId}", projectId);
                return Result.Failure<TeamProject>($"No project found with id {projectId}");
            }

            return Result.Success(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown getting project {ProjectId} from Azure DevOps", projectId);
            return Result.Failure<TeamProject>(ex.ToString());
        }
    }
}
