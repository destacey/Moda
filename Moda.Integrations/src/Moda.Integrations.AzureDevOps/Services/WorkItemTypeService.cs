using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace Moda.Integrations.AzureDevOps.Services;
internal sealed class WorkItemTypeService
{
    private readonly WorkItemTrackingHttpClient _witClient;
    private readonly ILogger<WorkItemTypeService> _logger;

    public WorkItemTypeService(VssConnection connection, ILogger<WorkItemTypeService> logger)
    {
        _witClient = Guard.Against.Null(connection.GetClient<WorkItemTrackingHttpClient>());
        _logger = logger;
    }

    public async Task<Result<List<WorkItemType>>> GetWorkItemTypes(Guid projectId, CancellationToken cancellationToken)
    {
        try
        {
            var types = await _witClient.GetWorkItemTypesAsync(projectId, cancellationToken: cancellationToken);
            if (types is null)
            {
                _logger.LogWarning("No work item types found for project {ProjectId}", projectId);
                return Result.Failure<List<WorkItemType>>($"No work item types found for project {projectId}");
            }

            _logger.LogDebug("{TypeCount} work item types found for project {ProjectId}", types.Count, projectId);

            return types;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work item types for project {ProjectId} from Azure DevOps", projectId);
            return Result.Failure<List<WorkItemType>>(ex.ToString());
        }
    }
}
