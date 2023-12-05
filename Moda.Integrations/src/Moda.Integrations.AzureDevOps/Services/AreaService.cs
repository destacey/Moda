using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using Moda.Common.Extensions;

namespace Moda.Integrations.AzureDevOps.Services;
internal sealed class AreaService
{
    private readonly WorkItemTrackingHttpClient _witClient;
    private readonly ILogger<AreaService> _logger;

    public AreaService(VssConnection connection, ILogger<AreaService> logger)
    {
        _witClient = Guard.Against.Null(connection.GetClient<WorkItemTrackingHttpClient>());
        _logger = logger;
    }

    public async Task<Result<List<WorkItemClassificationNode>>> GetAreas(Guid projectId, CancellationToken cancellationToken)
    {
        try
        {
            var rootArea = await _witClient.GetClassificationNodeAsync(projectId, TreeStructureGroup.Areas, depth: 100, cancellationToken: cancellationToken);
            if (rootArea is null)
            {
                _logger.LogWarning("No area node found for project {ProjectId}", projectId);
                var result = Result.Failure<List<WorkItemClassificationNode>>($"No area node found for project {projectId}");
                return result;
            }

            var areas = rootArea.FlattenHierarchy(a => a.Children).ToList();
            foreach (var item in areas)
            {
                item.Children = null;
            }

            _logger.LogDebug("{AreaCount} areas found for project {ProjectId}", areas.Count, projectId);

            return Result.Success(areas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown getting areas for project {ProjectId} from Azure DevOps", projectId);
            return Result.Failure<List<WorkItemClassificationNode>>(ex.ToString());
        }
    }
}
