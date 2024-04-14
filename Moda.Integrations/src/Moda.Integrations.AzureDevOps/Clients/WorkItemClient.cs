using System.Linq;
using Ardalis.GuardClauses;
using Moda.Integrations.AzureDevOps.Models.WorkItems;
using RestSharp;

namespace Moda.Integrations.AzureDevOps.Clients;
internal sealed class WorkItemClient : BaseClient
{
    internal WorkItemClient(string organizationUrl, string token, string apiVersion)
        : base(organizationUrl, token, apiVersion)
    { }

    internal async Task<List<int>> GetWorkItemIds(string project, DateTime lastChangedDate, string[] workItemTypes, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(project, nameof(project));

        var request = new RestRequest("/_apis/wit/wiql", Method.Post);
        SetupRequest(request);

        int maxResults = 20_000;
        request.AddQueryParameter("$top", maxResults);
        request.AddQueryParameter("timePrecision", "true");

        int startingId = 0;
        List<int> workItemIds = [];
        while (true)
        {
            var wiql = CreateWiqlQueryForSync(project, lastChangedDate, workItemTypes, startingId);

            request.AddJsonBody(wiql);

            var response = await _client.ExecuteAsync<WiqlResponse>(request, cancellationToken);
            if (!response.IsSuccessful)
            {
                throw new Exception($"Error getting work item ids for project {project} from Azure DevOps: {response.ErrorMessage}");
            }
            else if (response.Data?.WorkItems is null || response.Data.WorkItems.Count == 0)
            {
                break;
            }

            var ids = response.Data.WorkItems.Select(wi => wi.Id).ToList();
            workItemIds.AddRange(ids);

            if (ids.Count < maxResults)
            {
                break;
            }

            startingId = ids.Last();
            var bodyParameter = request.Parameters.OfType<BodyParameter>().Single();
            request.RemoveParameter(bodyParameter);
        }

        return workItemIds.Distinct().ToList();
    }

    private static WiqlRequest CreateWiqlQueryForSync(string project, DateTime dateTime, string[] workItemTypes, int startingId)
    {
        var workItemTypesFilter = "";
        if (workItemTypes.Length > 0)
        {
            workItemTypesFilter = $"AND [System.WorkItemType] IN ({string.Join(",", workItemTypes.Select(t => $"\'{t}\'"))})";
        }
        return new()
        {
            Query = $"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = '{project}' AND [System.Id] > {startingId} AND [System.ChangedDate] > '{dateTime:yyyy-MM-ddTHH:mm:ssZ}' {workItemTypesFilter} ORDER BY [System.Id] Asc"
        };
    }
}
