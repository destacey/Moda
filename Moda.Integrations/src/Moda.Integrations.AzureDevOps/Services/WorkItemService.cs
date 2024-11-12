using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Moda.Integrations.AzureDevOps.Clients;
using Moda.Integrations.AzureDevOps.Models.WorkItems;

namespace Moda.Integrations.AzureDevOps.Services;
internal sealed class WorkItemService(string organizationUrl, string token, string apiVersion, ILogger<WorkItemService> logger)
{
    private readonly WorkItemClient _workItemClient = new(organizationUrl, token, apiVersion);
    private readonly ILogger<WorkItemService> _logger = logger;

    public async Task<Result<List<WorkItemResponse>>> GetWorkItems(string projectName, DateTime lastChangedDate, string[] workItemTypes, CancellationToken cancellationToken)
    {
        try
        {
            var workItemIds = await _workItemClient.GetWorkItemIds(projectName, lastChangedDate, workItemTypes, cancellationToken);

            _logger.LogDebug("{WorkItemIdCount} work item ids found for project {Project}", workItemIds.Length, projectName);

            if (workItemIds.Length == 0)
            {
                return Result.Success<List<WorkItemResponse>>([]);
            }

            // TODO: add cancellation process

            // TODO: make this configurable
            string[] fields =
            [
                "System.CreatedDate",
                "System.CreatedBy",
                "System.ChangedDate",
                "System.ChangedBy",
                "System.State",
                "System.Title",
                "System.WorkItemType",

                "System.Parent",
                "System.AreaId",
                "System.AssignedTo",
                "System.IterationId",
                "Microsoft.VSTS.Common.Priority",
                "Microsoft.VSTS.Common.StackRank",
                "Microsoft.VSTS.Common.ActivatedDate",
                "Microsoft.VSTS.Common.ClosedDate"
            ];

            var workitems = await _workItemClient.GetWorkItems(projectName, workItemIds, fields, cancellationToken);

            _logger.LogDebug("{WorkItemCount} work items found for project {Project}", workitems.Count, projectName);

            return Result.Success(workitems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown getting work items for project {Project} from Azure DevOps", projectName);
            return Result.Failure<List<WorkItemResponse>>(ex.ToString());
        }
    }

    public async Task<Result<List<ReportingWorkItemLinkResponse>>> GetParentLinkChanges(string projectName, DateTime lastChangedDate, string[] workItemTypes, CancellationToken cancellationToken)
    {
        try
        {
            string[] linkTypes = ["System.LinkTypes.Hierarchy"];

            var links = await _workItemClient.GetWorkItemLinkChanges(projectName, lastChangedDate, linkTypes, workItemTypes, cancellationToken);

            _logger.LogDebug("{LinkCount} parent link changes found for project {Project}", links.Count, projectName);

            return Result.Success(links);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown getting parent link changes for project {Project} from Azure DevOps", projectName);
            return Result.Failure<List<ReportingWorkItemLinkResponse>>(ex.ToString());
        }
    }

    public async Task<Result<int[]>> GetDeletedWorkItemIds(string projectName, DateTime lastChangedDate, CancellationToken cancellationToken)
    {
        try
        {
            int[] workItemIds = await _workItemClient.GetDeletedWorkItemIds(projectName, cancellationToken);

            // TODO: make this configurable and better.
            // We are trying to solve for the scenario where a work item is synced, but then later
            // changes to a work item type that is not being synced.
            int[] noneSyncedIds = await _workItemClient.GetWorkItemIds(projectName, lastChangedDate, ["Task", "Issue"], cancellationToken);

            int[] deletedWorkItemIds = new int[workItemIds.Length + noneSyncedIds.Length];
            Array.Copy(workItemIds, 0, deletedWorkItemIds, 0, workItemIds.Length);
            Array.Copy(noneSyncedIds, 0, deletedWorkItemIds, workItemIds.Length, noneSyncedIds.Length);

            _logger.LogDebug("{WorkItemIdCount} deleted work item ids found for project {Project}", deletedWorkItemIds.Length, projectName);

            return Result.Success(deletedWorkItemIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown getting deleted work item ids for project {Project} from Azure DevOps", projectName);
            return Result.Failure<int[]>(ex.ToString());
        }
    }
}
