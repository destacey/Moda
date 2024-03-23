using Ardalis.GuardClauses;
using Moda.Integrations.AzureDevOps.Extensions;
using Moda.Integrations.AzureDevOps.Models;
using Moda.Integrations.AzureDevOps.Models.Contracts;
using Moda.Integrations.AzureDevOps.Models.Projects;
using RestSharp;

namespace Moda.Integrations.AzureDevOps.Clients;
internal sealed class ProjectClient : IDisposable
{
    private readonly RestClient _client;
    private readonly string _token;
    private readonly string _apiVersion;

    // TODO: add retry logic (with Polly?)
    internal ProjectClient(string organizationUrl, string token, string apiVersion)
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

    internal async Task<RestResponse<AzdoListResponse<ProjectDto>>> GetProjects(int top, int skip, CancellationToken cancellationToken)
    {
        var request = new RestRequest("/_apis/projects", Method.Get);
        SetupRequest(request);
        request.AddParameter("$top", top);
        request.AddParameter("$skip", skip);

        return await _client.ExecuteAsync<AzdoListResponse<ProjectDto>>(request, cancellationToken);
    }

    internal async Task<RestResponse<ProjectDto>> GetProject(Guid projectId, CancellationToken cancellationToken)
    {
        var request = new RestRequest($"/_apis/projects/{projectId}", Method.Get);
        SetupRequest(request);

        return await _client.ExecuteAsync<ProjectDto>(request, cancellationToken);
    }

    internal async Task<RestResponse<ListResponse<PropertyDto>>> GetProjectProperties(Guid projectId, CancellationToken cancellationToken)
    {
        var request = new RestRequest($"/_apis/projects/{projectId}/properties", Method.Get);
        SetupRequest(request, true);
        request.AddParameter("keys", "System.ProcessTemplateType");

        return await _client.ExecuteAsync<ListResponse<PropertyDto>>(request, cancellationToken);
    }

    private void SetupRequest(RestRequest request, bool includePreviewTag = false)
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
