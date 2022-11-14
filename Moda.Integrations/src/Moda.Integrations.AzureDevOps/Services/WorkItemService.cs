using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using Moda.Common.Extensions;

namespace Moda.Integrations.AzureDevOps.Services;
internal sealed class WorkItemService
{
    private readonly WorkItemTrackingHttpClient _witClient;
    private readonly ILogger<WorkItemService> _logger;
    private readonly int _maxBatchSize = 200;
    private readonly int _maxQueryBatchSize = 10000;

    public WorkItemService(VssConnection connection, ILogger<WorkItemService> logger)
    {
        _witClient = Guard.Against.Null(connection.GetClient<WorkItemTrackingHttpClient>());
        _logger = logger;
    }

    public async Task<Result<WorkItem>> GetWorkItem(Guid projectId, int workItemId, CancellationToken cancellationToken)
    {
        try
        {
            var workitem = await _witClient.GetWorkItemAsync(projectId, workItemId, expand: WorkItemExpand.All, cancellationToken: cancellationToken);
            if (workitem is null)
            {
                _logger.LogWarning("No work item found with id {WorkItemId} for {ProjectId}", workItemId, projectId);
                return Result.Failure<WorkItem>($"No work item found with id {workItemId} for {projectId}");
            }

            return Result.Success(workitem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work item for project {ProjectId} from Azure DevOps", projectId);
            return Result.Failure<WorkItem>(ex.ToString());
        }
    }

    public async Task<Result<List<WorkItem>>> GetWorkItems(Guid projectId, string projectName, CancellationToken cancellationToken)
    {
        try
        {
            List<string> fields = new() { "System.ChangedDate" };

            var wiql = CreateBaseGetAllWiql(projectName, new DateTime(1900, 1, 1));

            List<int> workItemIds = new();

            while (true)
            {
                var batchResult = await _witClient.QueryByWiqlAsync(wiql, projectId, timePrecision: true, top: _maxQueryBatchSize, cancellationToken: cancellationToken);

                workItemIds.AddRange(batchResult.WorkItems.Select(wi => wi.Id));

                if (batchResult.WorkItems?.Count() < _maxQueryBatchSize)
                    break;

                int lastWorkItemId = batchResult.WorkItems!.Last().Id;
                var lastWorkitem = await _witClient.GetWorkItemAsync(
                    project: projectId,
                    id: lastWorkItemId,
                    fields: fields,
                    cancellationToken: cancellationToken);

                if (lastWorkitem is null || !lastWorkitem.Fields.TryGetValue("System.ChangedDate", out object? changedDate))
                {
                    _logger.LogError("Error getting the changeDate for work item {LastWorkItemId} while querying for work items for project {rojectId}", lastWorkItemId, projectId);
                    return Result.Failure<List<WorkItem>>($"Error getting the changeDate for work item {lastWorkItemId} while querying for work items for project {projectId}");
                }
                wiql = CreateBaseGetAllWiql(projectName, (DateTime)changedDate);
            }

            if (!workItemIds.Any())
            {
                _logger.LogError("Error getting work items for project {ProjectId} from Azure DevOps", projectId);
                return Result.Failure<List<WorkItem>>($"Error getting work items for project {projectId} from Azure DevOps");
            }

            var workitems = new List<WorkItem>();

            var batches = workItemIds.Distinct().Batch(_maxBatchSize);
            foreach (var batch in batches)
            {
                var batchWorkitems = await _witClient.GetWorkItemsAsync(projectId, batch, expand: WorkItemExpand.All, cancellationToken: cancellationToken);
                workitems.AddRange(batchWorkitems);
            }

            _logger.LogDebug("{WorkItemCount} work items found for project {ProjectId}", workitems.Count, projectId);

            return Result.Success(workitems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work items for project {ProjectId} from Azure DevOps", projectId);
            return Result.Failure<List<WorkItem>>(ex.ToString());
        }
    }

    public async Task<Result<List<WorkItem>>> GetWorkItems(Guid projectId, int[] workItemIds, CancellationToken cancellationToken)
    {
        try
        {
            var workitems = new List<WorkItem>();
            
            var batches = workItemIds.Batch(_maxBatchSize);
            foreach (var batch in batches)
            {
                var batchWorkitems = await _witClient.GetWorkItemsAsync(projectId, batch, expand: WorkItemExpand.All, cancellationToken: cancellationToken);
                workitems.AddRange(batchWorkitems);
            }

            _logger.LogDebug("{WorkItemCount} work items found for project {ProjectId}", workitems.Count, projectId);

            return Result.Success(workitems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work items for project {ProjectId} from Azure DevOps", projectId);
            return Result.Failure<List<WorkItem>>(ex.ToString());
        }
    }

    private static Wiql CreateBaseGetAllWiql(string projectName, DateTime dateTime) => new()
    {
        // TODO: an exception occurs when querying with Date and Time
        // NOTE: Even if other columns are specified, only the ID & URL are available in the WorkItemReference
        Query = "Select [Id], [Changed Date] " +
                "From WorkItems " +
                "Where [Changed Date] >= '" + dateTime.ToString("yyyy-MM-dd h:mm tt") + "' " +
                "And [System.TeamProject] = '" + projectName + "' " +
                "Order By [Changed Date] Asc"
    };
}
