using Moda.Integrations.AzureDevOps.Models;
using Moda.Integrations.AzureDevOps.Models.Contracts;
using Moda.Integrations.AzureDevOps.Models.Projects;
using RestSharp;

namespace Moda.Integrations.AzureDevOps.Clients;
internal sealed class ProjectClient : BaseClient
{
    internal ProjectClient(string organizationUrl, string token, string apiVersion)
        : base(organizationUrl, token, apiVersion)
    { }

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
}
