using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using Moda.Common.Extensions;

namespace Moda.Integrations.AzureDevOps.Services;
internal sealed class IterationService
{
    private readonly WorkItemTrackingHttpClient _witClient;
    private readonly ILogger<IterationService> _logger;

    public IterationService(VssConnection connection, ILogger<IterationService> logger)
    {
        _witClient = Guard.Against.Null(connection.GetClient<WorkItemTrackingHttpClient>());
        _logger = logger;
    }

    public async Task<Result<List<WorkItemClassificationNode>>> GetIterations(Guid projectId, CancellationToken cancellationToken)
    {
        try
        {
            var rootIteration = await _witClient.GetClassificationNodeAsync(projectId, TreeStructureGroup.Iterations, depth: 100, cancellationToken: cancellationToken);
            if (rootIteration is null)
            {
                _logger.LogWarning("No iteration node found for project {ProjectId}", projectId);
                var result = Result.Failure<List<WorkItemClassificationNode>>($"No iteration node found for project {projectId}");
                return result;
            }

            var iterations = rootIteration.FlattenHierarchy(a => a.Children).ToList();
            foreach (var item in iterations)
            {
                item.Children = null;
            }

            _logger.LogDebug("{IterationCount} iterations found for project {ProjectId}", iterations.Count, projectId);

            return Result.Success(iterations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown getting iterations for project {ProjectId} from Azure DevOps", projectId);
            return Result.Failure<List<WorkItemClassificationNode>>(ex.ToString());
        }
    }
}
