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
            List<int> workItemIds = await _workItemClient.GetWorkItemIds(projectName, lastChangedDate, workItemTypes, cancellationToken);

            _logger.LogDebug("{WorkItemIdCount} work item ids found for project {Project}", workItemIds.Count, projectName);

            if (workItemIds.Count == 0)
            {
                return Result.Success<List<WorkItemResponse>>([]);
            }

            // TODO: add cancellation process

            // TODO: make this configurable
            List<string> fields =
            [
                "System.AreaPath",
                "System.ChangedDate",
                "System.CreatedDate",
                "System.IterationPath",
                "System.State",
                "System.Title",
                "System.WorkItemType",

                "Microsoft.VSTS.Common.Priority",
                "Microsoft.VSTS.Common.StackRank"
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
}
