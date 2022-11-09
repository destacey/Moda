using Ardalis.GuardClauses;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Moda.Integrations.AzureDevOps.Services;
internal sealed class WorkItemService
{
    private readonly WorkItemTrackingHttpClient _witClient;
    private readonly string _projectName;

    internal WorkItemService(WorkItemTrackingHttpClient witClient, string projectName)
    {
        _witClient = Guard.Against.Null(witClient);
        _projectName = Guard.Against.NullOrWhiteSpace(projectName);
    }

    internal async Task<WorkItem> GetWorkItem(int id, CancellationToken cancellationToken)
    {
        var workitem = await _witClient.GetWorkItemAsync(id, expand: WorkItemExpand.All, cancellationToken: cancellationToken);

        return workitem;
    }

    internal async Task<IReadOnlyList<WorkItem>> GetWorkItems(int[] workItemIds, CancellationToken cancellationToken)
    {
        var workitems = await _witClient.GetWorkItemsAsync(workItemIds, expand: WorkItemExpand.All, cancellationToken: cancellationToken);

        return workitems;
    }
}
