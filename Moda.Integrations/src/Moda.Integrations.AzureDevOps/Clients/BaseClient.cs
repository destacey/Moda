using System.Text.Json;
using Ardalis.GuardClauses;
using Moda.Integrations.AzureDevOps.Extensions;
using Moda.Integrations.AzureDevOps.Models.Converters;
using RestSharp;
using RestSharp.Serializers.Json;

namespace Moda.Integrations.AzureDevOps.Clients;
internal abstract class BaseClient : IDisposable
{
    protected readonly RestClient _client;
    protected readonly string _token;
    protected readonly string _apiVersion;

    // TODO: add retry logic (with Polly?)
    internal BaseClient(string organizationUrl, string token, string apiVersion)
    {
        Guard.Against.NullOrWhiteSpace(organizationUrl, nameof(organizationUrl));
        Guard.Against.NullOrWhiteSpace(token, nameof(token));
        Guard.Against.NullOrWhiteSpace(apiVersion, nameof(apiVersion));

        _token = token;
        _apiVersion = apiVersion;

        var options = new RestClientOptions(organizationUrl)
        {
            Timeout = TimeSpan.FromSeconds(300),
        };

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new ReportingWorkItemLinkResponseConverter() }
        };

        _client = new RestClient(options, configureSerialization: s => s.UseSystemTextJson(jsonSerializerOptions));
    }

    protected void SetupRequest(RestRequest request, bool includePreviewTag = false)
    {
        request.AddAcceptHeaderWithApiVersion(_apiVersion, includePreviewTag);
        request.AddAuthorizationHeaderForPersonalAccessToken(_token);
    }

    public void Dispose()
    {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
