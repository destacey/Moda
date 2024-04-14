using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Moda.Integrations.AzureDevOps.Clients;

namespace Moda.Integrations.AzureDevOps.Services;
internal sealed class WorkItemService(string organizationUrl, string token, string apiVersion, ILogger<WorkItemService> logger)
{
    private readonly WorkItemClient _workItemClient = new(organizationUrl, token, apiVersion);
    private readonly ILogger<WorkItemService> _logger = logger;
    private readonly int _maxBatchSize = 200;
    private readonly int _maxQueryBatchSize = 10000;

    //public async Task<Result<WorkItem>> GetWorkItem(Guid projectId, int workItemId, CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        var workitem = await _witClient.GetWorkItemAsync(projectId, workItemId, expand: WorkItemExpand.All, cancellationToken: cancellationToken);
    //        if (workitem is null)
    //        {
    //            _logger.LogWarning("No work item found with id {WorkItemId} for {ProjectId}", workItemId, projectId);
    //            return Result.Failure<WorkItem>($"No work item found with id {workItemId} for {projectId}");
    //        }

    //        return Result.Success(workitem);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Exception thrown getting work item for project {ProjectId} from Azure DevOps", projectId);
    //        return Result.Failure<WorkItem>(ex.ToString());
    //    }
    //}

    public async Task<Result<List<WorkItem>>> GetWorkItems(string projectName, DateTime lastChangedDate, string[] workItemTypes, CancellationToken cancellationToken)
    {
        try
        {

            var workItemIds = await _workItemClient.GetWorkItemIds(projectName, lastChangedDate, workItemTypes, cancellationToken);

            _logger.LogDebug("{WorkItemIdCount} work item ids found for project {Project}", workItemIds.Count, projectName);

            // TODO: add cancellation process

            //List<string> fields = new() { "System.ChangedDate" };
            var workitems = new List<WorkItem>();

            //var batches = workItemIds.Distinct().Batch(_maxBatchSize);
            //foreach (var batch in batches)
            //{
            //    var batchWorkitems = await _witClient.GetWorkItemsAsync(projectId, batch, expand: WorkItemExpand.All, cancellationToken: cancellationToken);
            //    workitems.AddRange(batchWorkitems);
            //}

            //_logger.LogDebug("{WorkItemCount} work items found for project {ProjectId}", workitems.Count, projectId);

            return Result.Success(workitems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown getting work items for project {Project} from Azure DevOps", projectName);
            return Result.Failure<List<WorkItem>>(ex.ToString());
        }
    }

    //public async Task<Result<List<WorkItem>>> GetWorkItems(Guid projectId, int[] workItemIds, CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        var workitems = new List<WorkItem>();

    //        var batches = workItemIds.Batch(_maxBatchSize);
    //        foreach (var batch in batches)
    //        {
    //            var batchWorkitems = await _witClient.GetWorkItemsAsync(projectId, batch, expand: WorkItemExpand.All, cancellationToken: cancellationToken);
    //            workitems.AddRange(batchWorkitems);
    //        }

    //        _logger.LogDebug("{WorkItemCount} work items found for project {ProjectId}", workitems.Count, projectId);

    //        return Result.Success(workitems);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Exception thrown getting work items for project {ProjectId} from Azure DevOps", projectId);
    //        return Result.Failure<List<WorkItem>>(ex.ToString());
    //    }
    //}

    //private static Wiql CreateBaseGetAllWiql(string projectName, DateTime dateTime) => new()
    //{
    //    // TODO: an exception occurs when querying with Date and Time
    //    // NOTE: Even if other columns are specified, only the ID & URL are available in the WorkItemReference
    //    Query = "Select [Id], [Changed Date] " +
    //            "From WorkItems " +
    //            "Where [Changed Date] >= '" + dateTime.ToString("yyyy-MM-dd h:mm tt") + "' " +
    //            "And [System.TeamProject] = '" + projectName + "' " +
    //            "Order By [Changed Date] Asc"
    //};
}
