using Ardalis.GuardClauses;
using Moda.Integrations.AzureDevOps.Extensions;
using Moda.Integrations.AzureDevOps.Models.Contracts;
using Moda.Integrations.AzureDevOps.Models.Processes;
using RestSharp;

namespace Moda.Integrations.AzureDevOps.Clients;
internal sealed class ProcessClient : IDisposable
{
    private readonly RestClient _client;
    private readonly string _token;
    private readonly string _apiVersion;

    // TODO: add retry logic (with Polly?)
    internal ProcessClient(string organizationUrl, string token, string apiVersion)
    {
        Guard.Against.NullOrWhiteSpace(organizationUrl, nameof(organizationUrl));
        Guard.Against.NullOrWhiteSpace(token, nameof(token));
        Guard.Against.NullOrWhiteSpace(apiVersion, nameof(apiVersion));

        _token = token;
        _apiVersion = apiVersion;

        var options = new RestClientOptions(organizationUrl)
        {
            MaxTimeout = 300_000,
        };

        _client = new RestClient(options);
    }

    internal async Task<RestResponse<AzdoListResponse<ProcessDto>>> GetProcesses(CancellationToken cancellationToken)
    {
        var request = new RestRequest("/_apis/work/processes", Method.Get);
        SetupRequest(request);
        request.AddParameter("$expand", "projects");

        return await _client.ExecuteAsync<AzdoListResponse<ProcessDto>>(request, cancellationToken);
    }

    internal async Task<RestResponse<ProcessDto>> GetProcess(Guid processId, CancellationToken cancellationToken)
    {
        var request = new RestRequest($"/_apis/work/processes/{processId}", Method.Get);
        SetupRequest(request);
        request.AddParameter("$expand", "projects");

        return await _client.ExecuteAsync<ProcessDto>(request, cancellationToken);
    }

    internal async Task<RestResponse<AzdoListResponse<ProcessWorkItemTypeDto>>> GetWorkItemTypes(Guid processId, CancellationToken cancellationToken)
    {
        var request = new RestRequest($"/_apis/work/processes/{processId}/workitemtypes", Method.Get);
        SetupRequest(request);
        request.AddParameter("$expand", "states,behaviors");

        return await _client.ExecuteAsync<AzdoListResponse<ProcessWorkItemTypeDto>>(request, cancellationToken);
    }

    internal async Task<RestResponse<AzdoListResponse<BehaviorDto>>> GetBehaviors(Guid processId, CancellationToken cancellationToken)
    {
        var request = new RestRequest($"/_apis/work/processes/{processId}/behaviors", Method.Get);
        SetupRequest(request);

        return await _client.ExecuteAsync<AzdoListResponse<BehaviorDto>>(request, cancellationToken);
    }

    private void SetupRequest(RestRequest request)
    {
        request.AddAcceptHeaderWithApiVersion(_apiVersion);
        request.AddAuthorizationHeaderForPersonalAccessToken(_token);
    }

    public void Dispose()
    {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
