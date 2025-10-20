using Moda.Integrations.AzureDevOps.Models;
using RestSharp;

namespace Moda.Integrations.AzureDevOps.Clients;
internal sealed class GeneralClient : BaseClient
{
    internal GeneralClient(string organizationUrl, string token, string apiVersion)
        : base(organizationUrl, token, apiVersion)
    { }

    internal async Task<RestResponse<ConnectionDataResponse>> GetConnectionData(CancellationToken cancellationToken)
    {
        var request = new RestRequest("/_apis/connectionData", Method.Get);
        SetupRequest(request, true);  // still in preview only

        return await _client.ExecuteAsync<ConnectionDataResponse>(request, cancellationToken)
            .ConfigureAwait(false);
    }
}
