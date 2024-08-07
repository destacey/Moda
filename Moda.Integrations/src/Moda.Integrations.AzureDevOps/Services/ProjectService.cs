﻿using System.Collections.Generic;
using System.Net;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Extensions;
using Moda.Integrations.AzureDevOps.Clients;
using Moda.Integrations.AzureDevOps.Models;
using Moda.Integrations.AzureDevOps.Models.Contracts;
using Moda.Integrations.AzureDevOps.Models.Projects;

namespace Moda.Integrations.AzureDevOps.Services;
internal sealed class ProjectService(string organizationUrl, string token, string apiVersion, ILogger<ProjectService> logger)
{
    private readonly ProjectClient _projectClient = new(organizationUrl, token, apiVersion);
    private readonly ILogger<ProjectService> _logger = logger;
    private readonly int _maxBatchSize = 100;

    public async Task<Result<List<ProjectDto>>> GetProjects(CancellationToken cancellationToken)
    {
        try
        {
            List<ProjectDto> projects = [];

            while (true)
            {
                var batch = await _projectClient.GetProjects(top: _maxBatchSize, skip: projects.Count, cancellationToken);
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

            _logger.LogDebug("{ProjectCount} projects found ", projects.Count);

            return projects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown getting projects from Azure DevOps");
            return Result.Failure<List<ProjectDto>>(ex.ToString());
        }
    }

    public async Task<Result<ProjectDetailsDto>> GetProject(Guid projectId, CancellationToken cancellationToken)
    {
        try
        {
            var projectResponse = await _projectClient.GetProject(projectId, cancellationToken);
            if (!projectResponse.IsSuccessful && projectResponse.StatusCode != HttpStatusCode.NotFound)
            {
                var statusDescription = projectResponse.StatusCode is 0 ? "Connection Error" : projectResponse.StatusDescription;
                var errorMessage = projectResponse.ErrorMessage is null ? statusDescription : $"{statusDescription} - {projectResponse.ErrorMessage}";
                _logger.LogError("Error getting project {ProjectId} from Azure DevOps: {ErrorMessage}.", projectId, errorMessage);
                return Result.Failure<ProjectDetailsDto>(errorMessage);
            }
            else if ((!projectResponse.IsSuccessful && projectResponse.StatusCode is HttpStatusCode.NotFound) || projectResponse.Data is null)
            {
                var errorMesssage = projectResponse.IsSuccessful ? "No project data returned" : projectResponse.StatusDescription;
                _logger.LogError("Error getting project {ProjectId} from Azure DevOps: {ErrorMessage}.", projectId, errorMesssage);
                return Result.Failure<ProjectDetailsDto>(errorMesssage);
            }

            var propertiesResponse = await _projectClient.GetProjectProperties(projectId, cancellationToken);
            if (!propertiesResponse.IsSuccessful && propertiesResponse.StatusCode != HttpStatusCode.NotFound)
            {
                var statusDescription = propertiesResponse.StatusCode is 0 ? "Connection Error" : propertiesResponse.StatusDescription;
                var errorMessage = propertiesResponse.ErrorMessage is null ? statusDescription : $"{statusDescription} - {propertiesResponse.ErrorMessage}";
                _logger.LogError("Error getting project properties {ProjectId} from Azure DevOps: {ErrorMessage}.", projectId, errorMessage);
                return Result.Failure<ProjectDetailsDto>(errorMessage);
            }
            else if ((!propertiesResponse.IsSuccessful && propertiesResponse.StatusCode is HttpStatusCode.NotFound) || propertiesResponse.Data is null)
            {
                var errorMesssage = propertiesResponse.IsSuccessful ? "No project properties data returned" : propertiesResponse.StatusDescription;
                _logger.LogError("Error getting project properties {ProjectId} from Azure DevOps: {ErrorMessage}.", projectId, errorMesssage);
                return Result.Failure<ProjectDetailsDto>(errorMesssage);
            }

            return ProjectDetailsDto.Create(projectResponse.Data, [.. propertiesResponse.Data.Value]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown getting project {ProjectId} from Azure DevOps", projectId);
            return Result.Failure<ProjectDetailsDto>(ex.ToString());
        }
    }

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
                var response = await _projectClient.GetProjectTeams(id, cancellationToken);
                if (!response.IsSuccessful)
                {
                    _logger.LogError("Error getting teams for project {ProjectId} from Azure DevOps: {ErrorMessage}.", id, response.ErrorMessage);
                    continue;
                }
                if (response.Data is null)
                {
                    _logger.LogDebug("No teams found for project {ProjectId}.", id);
                    continue;
                }

                teams.AddRange(response.Data.Value.ToIExternalTeams(id));
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

    public async Task<Result<List<ClassificationNodeResponse>>> GetAreaPaths(string projectName, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _projectClient.GetAreaPaths(projectName, cancellationToken);
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

            _logger.LogDebug("{AreaCount} areas found for project {ProjectId}.", areaPaths.Count, projectName);

            return areaPaths;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown getting areas for project {ProjectId} from Azure DevOps", projectName);
            return Result.Failure<List<ClassificationNodeResponse>> (ex.ToString());
        }
    }
}
