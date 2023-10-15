using Ardalis.GuardClauses;
using Moda.Integrations.AzureDevOps.Extensions;
using Moda.Integrations.AzureDevOps.Models;
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

    internal async Task<RestResponse<GetProcessesResponse>> GetProcesses(CancellationToken cancellationToken)
    {
        var request = new RestRequest("/_apis/work/processes", Method.Get);
        request.AddAcceptHeaderWithApiVersion(_apiVersion);
        request.AddAuthorizationHeaderForPersonalAccessToken(_token);
        request.AddParameter("$expand", "projects");

        return await _client.ExecuteAsync<GetProcessesResponse>(request, cancellationToken);
    }

    public void Dispose()
    {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
