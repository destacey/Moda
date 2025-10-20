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

        return await _client.ExecuteAsync<AzdoListResponse<ProjectDto>>(request, cancellationToken).ConfigureAwait(false);
    }

    internal async Task<RestResponse<ProjectDto>> GetProject(string projectIdOrName, CancellationToken cancellationToken)
    {
        var request = new RestRequest($"/_apis/projects/{projectIdOrName}", Method.Get);
        SetupRequest(request);

        return await _client.ExecuteAsync<ProjectDto>(request, cancellationToken);
    }

    internal async Task<RestResponse<ListResponse<PropertyDto>>> GetProjectProperties(Guid projectId, CancellationToken cancellationToken)
    {
        var request = new RestRequest($"/_apis/projects/{projectId}/properties", Method.Get);
        SetupRequest(request, true);
        request.AddParameter("keys", "System.ProcessTemplateType");

        return await _client.ExecuteAsync<ListResponse<PropertyDto>>(request, cancellationToken).ConfigureAwait(false);
    }

    internal async Task<RestResponse<ListResponse<TeamDto>>> GetProjectTeams(Guid projectId, CancellationToken cancellationToken)
    {
        var request = new RestRequest($"/_apis/projects/{projectId}/teams", Method.Get);
        SetupRequest(request);

        return await _client.ExecuteAsync<ListResponse<TeamDto>>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the team settings for the specified team.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="teamId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal async Task<RestResponse<TeamSettingsResponse>> GetProjectTeamsSettings(Guid projectId, Guid teamId, CancellationToken cancellationToken)
    {
        var request = new RestRequest($"/{projectId}/{teamId}/_apis/work/teamsettings", Method.Get);
        SetupRequest(request);

        return await _client.ExecuteAsync<TeamSettingsResponse>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the root area path and all child area paths for the project.
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal async Task<RestResponse<ClassificationNodeResponse>> GetAreaPaths(string projectName, CancellationToken cancellationToken)
    {
        var request = new RestRequest($"/{projectName}/_apis/wit/classificationnodes/areas", Method.Get);
        SetupRequest(request);
        request.AddParameter("$depth", 100); // TODO: make this configurable

        return await _client.ExecuteAsync<ClassificationNodeResponse>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the root iteration path and all child iteration paths for the project.
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal async Task<RestResponse<IterationNodeResponse>> GetIterationPaths(string projectName, CancellationToken cancellationToken)
    {
        var request = new RestRequest($"/{projectName}/_apis/wit/classificationnodes/iterations", Method.Get);
        SetupRequest(request);
        request.AddParameter("$depth", 100); // TODO: make this configurable

        return await _client.ExecuteAsync<IterationNodeResponse>(request, cancellationToken).ConfigureAwait(false);
    }
}
