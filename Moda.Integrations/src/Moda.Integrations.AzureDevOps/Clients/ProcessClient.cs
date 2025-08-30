using Moda.Integrations.AzureDevOps.Models.Contracts;
using Moda.Integrations.AzureDevOps.Models.Processes;
using RestSharp;

namespace Moda.Integrations.AzureDevOps.Clients;
internal sealed class ProcessClient : BaseClient
{
    internal ProcessClient(string organizationUrl, string token, string apiVersion)
        : base(organizationUrl, token, apiVersion)
    { }

    internal async Task<RestResponse<AzdoListResponse<ProcessDto>>> GetProcesses(CancellationToken cancellationToken)
    {
        var request = new RestRequest("/_apis/work/processes", Method.Get);
        SetupRequest(request);
        request.AddParameter("$expand", "projects");

        return await _client.ExecuteAsync<AzdoListResponse<ProcessDto>>(request, cancellationToken).ConfigureAwait(false);
    }

    internal async Task<RestResponse<ProcessDto>> GetProcess(Guid processId, CancellationToken cancellationToken)
    {
        var request = new RestRequest($"/_apis/work/processes/{processId}", Method.Get);
        SetupRequest(request);
        request.AddParameter("$expand", "projects");

        return await _client.ExecuteAsync<ProcessDto>(request, cancellationToken).ConfigureAwait(false);
    }

    internal async Task<RestResponse<AzdoListResponse<ProcessWorkItemTypeDto>>> GetWorkItemTypes(Guid processId, CancellationToken cancellationToken)
    {
        var request = new RestRequest($"/_apis/work/processes/{processId}/workitemtypes", Method.Get);
        SetupRequest(request);
        request.AddParameter("$expand", "states,behaviors");

        // there seems to be a bug in the API when including states that duplicates hidden states

        return await _client.ExecuteAsync<AzdoListResponse<ProcessWorkItemTypeDto>>(request, cancellationToken).ConfigureAwait(false);
    }

    internal async Task<RestResponse<AzdoListResponse<ProcessWorkItemStateDto>>> GetWorkItemTypeStates(Guid processId, string workItemTypeReferenceName, CancellationToken cancellationToken)
    {
        var request = new RestRequest($"/_apis/work/processes/{processId}/workitemtypes/{workItemTypeReferenceName}/states", Method.Get);
        SetupRequest(request);

        return await _client.ExecuteAsync<AzdoListResponse<ProcessWorkItemStateDto>>(request, cancellationToken).ConfigureAwait(false);
    }

    internal async Task<RestResponse<AzdoListResponse<BehaviorDto>>> GetBehaviors(Guid processId, CancellationToken cancellationToken)
    {
        var request = new RestRequest($"/_apis/work/processes/{processId}/behaviors", Method.Get);
        SetupRequest(request);

        return await _client.ExecuteAsync<AzdoListResponse<BehaviorDto>>(request, cancellationToken).ConfigureAwait(false);
    }
}
