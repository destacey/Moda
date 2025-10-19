using System.Net;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Extensions;
using Moda.Integrations.AzureDevOps.Clients;
using Moda.Integrations.AzureDevOps.Models;
using Moda.Integrations.AzureDevOps.Models.Projects;

namespace Moda.Integrations.AzureDevOps.Services;
internal sealed class ProjectService(string organizationUrl, string token, string apiVersion, ILogger<ProjectService> logger)
{
    private readonly ProjectClient _projectClient = new(organizationUrl, token, apiVersion);
    private readonly ILogger<ProjectService> _logger = logger;
    private readonly int _maxBatchSize = 100;

    /// <summary>
    /// Retrieves a list of projects from Azure DevOps in batches.
    /// </summary>
    /// <remarks>This method fetches projects in batches to handle large datasets efficiently. It continues
    /// retrieving batches until all projects are fetched or an error occurs. If an error occurs during the retrieval
    /// process, the method logs the error and returns a failure result.</remarks>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The operation will terminate early if the token is canceled.</param>
    /// <returns>A <see cref="Result{T}"/> containing a list of <see cref="ProjectDto"/> objects if the operation is successful;
    /// otherwise, a failure result with an error message.</returns>
    public async Task<Result<List<ProjectDto>>> GetProjects(CancellationToken cancellationToken)
    {
        try
        {
            List<ProjectDto> projects = [];

            while (true)
            {
                var batch = await _projectClient.GetProjects(top: _maxBatchSize, skip: projects.Count, cancellationToken).ConfigureAwait(false);
                if (!batch.IsSuccessful)
                {
                    _logger.LogError("Error getting projects from Azure DevOps: {ErrorMessage}.", batch.ErrorMessage);
                    return Result.Failure<List<ProjectDto>>(batch.ErrorMessage);
                }

                if (batch.Data is null)
                    break;

                projects.AddRange(batch.Data.Items);

                if (batch.Data.Count < _maxBatchSize)
                    break;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("{ProjectCount} projects found ", projects.Count);

            return projects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown getting projects from Azure DevOps");
            return Result.Failure<List<ProjectDto>>(ex.ToString());
        }
    }

    /// <summary>
    /// Retrieves the details of a project, including its properties, from Azure DevOps.
    /// </summary>
    /// <remarks>This method fetches the project details and its associated properties from Azure DevOps. If
    /// the project or its properties cannot be retrieved due to an error or if they do not exist, the method logs the
    /// error and returns a failure result.</remarks>
    /// <param name="projectIdOrName">The unique identifier or name of the project to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}"/> containing the project details as a <see cref="ProjectDetailsDto"/> if successful;
    /// otherwise, a failure result with an error message.</returns>
    public async Task<Result<ProjectDetailsDto>> GetProject(string projectIdOrName, CancellationToken cancellationToken)
    {
        try
        {
            var projectResponse = await _projectClient.GetProject(projectIdOrName, cancellationToken).ConfigureAwait(false);
            if (!projectResponse.IsSuccessful && projectResponse.StatusCode != HttpStatusCode.NotFound)
            {
                var statusDescription = projectResponse.StatusCode is 0 ? "Connection Error" : projectResponse.StatusDescription;
                var errorMessage = projectResponse.ErrorMessage is null ? statusDescription : $"{statusDescription} - {projectResponse.ErrorMessage}";
                _logger.LogError("Error getting project {ProjectIdOrName} from Azure DevOps: {ErrorMessage}.", projectIdOrName, errorMessage);
                return Result.Failure<ProjectDetailsDto>(errorMessage);
            }
            else if ((!projectResponse.IsSuccessful && projectResponse.StatusCode is HttpStatusCode.NotFound) || projectResponse.Data is null)
            {
                var errorMesssage = projectResponse.IsSuccessful ? "No project data returned" : projectResponse.StatusDescription;
                _logger.LogError("Error getting project {ProjectIdOrName} from Azure DevOps: {ErrorMessage}.", projectIdOrName, errorMesssage);
                return Result.Failure<ProjectDetailsDto>(errorMesssage);
            }

            var propertiesResponse = await _projectClient.GetProjectProperties(projectResponse.Data.Id, cancellationToken).ConfigureAwait(false);
            if (!propertiesResponse.IsSuccessful && propertiesResponse.StatusCode != HttpStatusCode.NotFound)
            {
                var statusDescription = propertiesResponse.StatusCode is 0 ? "Connection Error" : propertiesResponse.StatusDescription;
                var errorMessage = propertiesResponse.ErrorMessage is null ? statusDescription : $"{statusDescription} - {propertiesResponse.ErrorMessage}";
                _logger.LogError("Error getting project properties {ProjectId} from Azure DevOps: {ErrorMessage}.", projectIdOrName, errorMessage);
                return Result.Failure<ProjectDetailsDto>(errorMessage);
            }
            else if ((!propertiesResponse.IsSuccessful && propertiesResponse.StatusCode is HttpStatusCode.NotFound) || propertiesResponse.Data is null)
            {
                var errorMesssage = propertiesResponse.IsSuccessful ? "No project properties data returned" : propertiesResponse.StatusDescription;
                _logger.LogError("Error getting project properties {ProjectId} from Azure DevOps: {ErrorMessage}.", projectIdOrName, errorMesssage);
                return Result.Failure<ProjectDetailsDto>(errorMesssage);
            }

            return ProjectDetailsDto.Create(projectResponse.Data, [.. propertiesResponse.Data.Value]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown getting project {ProjectId} from Azure DevOps", projectIdOrName);
            return Result.Failure<ProjectDetailsDto>(ex.ToString());
        }
    }

    /// <summary>
    /// Retrieves the teams for the specified project IDs from Azure DevOps.
    /// </summary>
    /// <param name="projectIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<List<IExternalTeam>>> GetTeams(Guid[] projectIds, CancellationToken cancellationToken)
    {
        List<IExternalTeam> teams = [];

        if (projectIds is null || projectIds.Length == 0)
        {
            _logger.LogWarning("No project ids provided to get teams from Azure DevOps.");
            return teams;
        }

        Guid currentProjectId = Guid.Empty;

        try
        {
            foreach (var id in projectIds)
            {
                currentProjectId = id;
                var response = await _projectClient.GetProjectTeams(id, cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccessful)
                {
                    _logger.LogError("Error getting teams for project {ProjectId} from Azure DevOps: {ErrorMessage}.", id, response.ErrorMessage);
                    continue;
                }
                if (response.Data is null)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug("No teams found for project {ProjectId}.", id);
                    continue;
                }

                // set team settings: boardId
                foreach (var team in response.Data.Value)
                {
                    var teamSettingsResponse = await _projectClient.GetProjectTeamsSettings(id, team.Id, cancellationToken).ConfigureAwait(false);
                    if (!teamSettingsResponse.IsSuccessful)
                    {
                        _logger.LogError("Error getting team settings for team {TeamId} in project {ProjectId} from Azure DevOps: {ErrorMessage}.", team.Id, id, teamSettingsResponse.ErrorMessage);
                        continue;
                    }
                    if (teamSettingsResponse.Data is null)
                    {
                        _logger.LogWarning("No team settings found for team {TeamId} in project {ProjectId}.", team.Id, id);
                        continue;
                    }

                    teams.Add(team.ToAzdoTeam(id, teamSettingsResponse.Data.BacklogIteration?.Id));
                }

                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("{TeamCount} teams found for project {ProjectId}.", response.Data.Value.Count, id);
            }

            return teams;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown getting teams for project {ProjectId} from Azure DevOps", currentProjectId);
            return Result.Failure<List<IExternalTeam>>(ex.ToString());
        }
    }

    /// <summary>
    /// Retrieves the area paths for a specified project from Azure DevOps.
    /// </summary>
    /// <remarks>This method retrieves the hierarchical area paths for the specified project by querying Azure
    /// DevOps. If the operation is unsuccessful, the result will contain an error message. If no area paths are found,
    /// the result will indicate failure with an appropriate message.</remarks>
    /// <param name="projectName">The name of the project for which to retrieve area paths. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The result contains a <see
    /// cref="Result{T}"/> object that holds a list of <see cref="ClassificationNodeResponse"/> representing the area
    /// paths if successful, or an error message if the operation fails.</returns>
    public async Task<Result<List<ClassificationNodeResponse>>> GetAreaPaths(string projectName, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _projectClient.GetAreaPaths(projectName, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessful)
            {
                _logger.LogError("Error getting areas for project {ProjectId} from Azure DevOps: {ErrorMessage}.", projectName, response.ErrorMessage);
                return Result.Failure<List<ClassificationNodeResponse>>(response.ErrorMessage);
            }
            if (response.Data is null)
            {
                _logger.LogWarning("No areas found for project {ProjectId}.", projectName);
                return Result.Failure<List<ClassificationNodeResponse>>($"No areas found for project {projectName}");
            }

            var areaPaths = response.Data.FlattenHierarchy(a => a.Children).ToList();

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("{AreaCount} areas found for project {ProjectId}.", areaPaths.Count, projectName);

            return areaPaths;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown getting areas for project {ProjectId} from Azure DevOps", projectName);
            return Result.Failure<List<ClassificationNodeResponse>>(ex.ToString());
        }
    }

    /// <summary>
    /// Retrieves a list of iterations for the specified project, optionally mapping iterations to team settings.
    /// </summary>
    /// <remarks>This method retrieves iteration paths from Azure DevOps for the specified project. If team
    /// settings are provided, the method maps iterations to the corresponding teams. The method logs errors and
    /// warnings for unsuccessful operations or when no iterations are found.</remarks>
    /// <param name="projectName">The name of the project for which to retrieve iterations. Cannot be null or empty.</param>
    /// <param name="teamSettings">An optional dictionary mapping team IDs to their corresponding iteration IDs. If provided, the method will
    /// associate iterations with the specified teams.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The result contains a <see
    /// cref="Result{T}"/> object that holds a list of <see cref="IterationDto"/> instances if successful, or an error
    /// message if the operation fails.</returns>
    public async Task<Result<List<IterationDto>>> GetIterations(string projectName, Dictionary<Guid, Guid?>? teamSettings, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _projectClient.GetIterationPaths(projectName, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessful)
            {
                _logger.LogError("Error getting iterations for project {ProjectId} from Azure DevOps: {ErrorMessage}.", projectName, response.ErrorMessage);
                return Result.Failure<List<IterationDto>>(response.ErrorMessage);
            }
            if (response.Data is null)
            {
                _logger.LogWarning("No iterations found for project {ProjectId}.", projectName);
                return Result.Failure<List<IterationDto>>($"No iterations found for project {projectName}");
            }

            Dictionary<Guid, Guid> iterationTeamMapping = ConvertTeamSettingsToIterationTeamMapping(teamSettings);

            var iterations = FlattenAndSetTeamIds(response.Data, iterationTeamMapping).ToList();

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("{IterationCount} iterations found for project {ProjectId}.", iterations.Count, projectName);

            return iterations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown getting iterations for project {ProjectId} from Azure DevOps", projectName);
            return Result.Failure<List<IterationDto>>(ex.ToString());
        }
    }

    /// <summary>
    /// Performs a single-pass traversal of the iteration hierarchy to flatten it and assign team IDs.
    /// This optimized approach eliminates the need for intermediate DTO conversions and multiple tree traversals.
    /// </summary>
    /// <param name="root">The root iteration node from Azure DevOps</param>
    /// <param name="iterationTeamMapping">Mapping of iteration IDs to team IDs</param>
    /// <returns>Flattened list of iterations with team IDs assigned</returns>
    private static IEnumerable<IterationDto> FlattenAndSetTeamIds(
        IterationNodeResponse root,
        Dictionary<Guid, Guid> iterationTeamMapping)
    {
        Stack<(IterationNodeResponse Node, Guid? ParentTeamId)> stack = new();
        stack.Push((root, null));

        while (stack.Count > 0)
        {
            var (current, parentTeamId) = stack.Pop();

            // Determine team ID: use mapped value if exists, otherwise inherit from parent
            var teamId = iterationTeamMapping.TryGetValue(current.Identifier, out var mappedTeamId)
                ? mappedTeamId
                : parentTeamId;

            // Yield flattened result immediately - no intermediate allocations
            yield return IterationDto.FromIterationNodeResponse(current, teamId);

            // Push children onto stack with inherited team ID
            if (current.Children is not null)
            {
                foreach (var child in current.Children)
                {
                    stack.Push((child, teamId));
                }
            }
        }
    }

    private Dictionary<Guid, Guid> ConvertTeamSettingsToIterationTeamMapping(Dictionary<Guid, Guid?>? teamSettings)
    {
        if (teamSettings is null || teamSettings.Count == 0)
            return [];

        var iterationTeamMapping = new Dictionary<Guid, Guid>(teamSettings.Count);

        foreach (var kv in teamSettings)
        {
            var teamId = kv.Key;
            var iterationId = kv.Value;

            if (iterationId is null)
                continue;

            if (!iterationTeamMapping.TryAdd(iterationId.Value, teamId))
            {
                _iterationAlreadyMapped(_logger, iterationId.Value, teamId, null);
            }
        }

        return iterationTeamMapping;
    }

    // Cached logger delegate to avoid per-call allocations/boxing
    // TODO: How do we manage EventId values?
    private static readonly Action<ILogger, Guid, Guid, Exception?> _iterationAlreadyMapped =
        LoggerMessage.Define<Guid, Guid>(LogLevel.Warning, new EventId(100001, "DuplicateIterationTeamMapping"),
            "Iteration {IterationId} is already mapped to team {TeamId}.");
}
