﻿using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Moda.Common.Application.Interfaces;
using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Interfaces.Work;
using Moda.Integrations.AzureDevOps.Models.Projects;
using Moda.Integrations.AzureDevOps.Models.WorkItems;
using Moda.Integrations.AzureDevOps.Services;

namespace Moda.Integrations.AzureDevOps;

public class AzureDevOpsService(ILogger<AzureDevOpsService> logger, IServiceProvider serviceProvider) : IAzureDevOpsService
{
    // https://learn.microsoft.com/en-us/azure/devops/integrate/concepts/rest-api-versioning?view=azure-devops#supported-versions
    private readonly string _apiVersion = "7.0";

    private readonly ILogger<AzureDevOpsService> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<Result> TestConnection(string organizationUrl, string token)
    {
        try
        {
            // use the GetWorkProcesses method to test the connection
            var result = await GetWorkProcesses(organizationUrl, token, CancellationToken.None);

            return result.IsSuccess
                ? Result.Success()
                : Result.Failure("Unable to verify connection.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown testing Azure DevOps connection.");
            return Result.Failure(ex.InnerException?.Message ?? ex.Message);
        }
    }

    public async Task<Result<List<IExternalWorkProcess>>> GetWorkProcesses(string organizationUrl, string token, CancellationToken cancellationToken)
    {
        var processService = GetService<ProcessService>(organizationUrl, token);

        var result = await processService.GetProcesses(cancellationToken);
        return result.IsSuccess
            ? Result.Success(result.Value.ToList<IExternalWorkProcess>())
            : Result.Failure<List<IExternalWorkProcess>>(result.Error);
    }

    public async Task<Result<IExternalWorkProcessConfiguration>> GetWorkProcess(string organizationUrl, string token, Guid processId, CancellationToken cancellationToken)
    {
        var processService = GetService<ProcessService>(organizationUrl, token);

        var result = await processService.GetProcess(processId, cancellationToken);
        return result.IsSuccess
            ? result.Value
            : Result.Failure<IExternalWorkProcessConfiguration>(result.Error);
    }

    public async Task<Result<IExternalWorkspaceConfiguration>> GetWorkspace(string organizationUrl, string token, Guid workspaceId, CancellationToken cancellationToken)
    {
        var projectService = GetService<ProjectService>(organizationUrl, token);

        var result = await projectService.GetProject(workspaceId, cancellationToken);

        if (result.IsFailure)
            return Result.Failure<IExternalWorkspaceConfiguration>(result.Error);

        if (!result.Value.HasProcessTemplateType)
            return Result.Failure<IExternalWorkspaceConfiguration>("Workspace does not have a process template type.");

        return Result.Success<IExternalWorkspaceConfiguration>(result.Value.ToAzdoWorkspaceConfiguration());
    }

    public async Task<Result<List<IExternalWorkspace>>> GetWorkspaces(string organizationUrl, string token, CancellationToken cancellationToken)
    {
        var projectService = GetService<ProjectService>(organizationUrl, token);

        var result = await projectService.GetProjects(cancellationToken);
        if (result.IsFailure)
            return Result.Failure<List<IExternalWorkspace>>(result.Error);

        return Result.Success(result.Value.ToAzdoWorkspaces().ToList<IExternalWorkspace>());
    }

    public async Task<Result<List<IExternalTeam>>> GetTeams(string organizationUrl, string token, Guid[] projectIds, CancellationToken cancellationToken)
    {
        var projectService = GetService<ProjectService>(organizationUrl, token);

        var result = await projectService.GetTeams(projectIds, cancellationToken);
        if (result.IsFailure)
            return Result.Failure<List<IExternalTeam>>(result.Error);

        _logger.LogDebug("{TeamCount} teams found for organization {organizationUrl}.", result.Value.Count, organizationUrl);

        return Result.Success(result.Value);
    }

    public async Task<Result<List<IExternalWorkItem>>> GetWorkItems(string organizationUrl, string token, string projectName, DateTime lastChangedDate, string[] workItemTypes, Dictionary<Guid,Guid?> teamSettings, CancellationToken cancellationToken)
    {
        var projectService = GetService<ProjectService>(organizationUrl, token);

        var iterationsResult = await projectService.GetIterations(projectName, teamSettings, cancellationToken);
        if (iterationsResult.IsFailure)
            return Result.Failure<List<IExternalWorkItem>>(iterationsResult.Error);

        var workItemService = GetService<WorkItemService>(organizationUrl, token);

        var result = await workItemService.GetWorkItems(projectName, lastChangedDate, workItemTypes, cancellationToken);

        return result.IsSuccess
            ? Result.Success(result.Value.ToIExternalWorkItems(iterationsResult.Value))
            : Result.Failure<List<IExternalWorkItem>>(result.Error);
    }

    public async Task<Result<List<IExternalWorkItemLink>>> GetParentLinkChanges(string organizationUrl, string token, string projectName, DateTime lastChangedDate, string[] workItemTypes, CancellationToken cancellationToken)
    {
        var workItemService = GetService<WorkItemService>(organizationUrl, token);

        var result = await workItemService.GetParentLinkChanges(projectName, lastChangedDate, workItemTypes, cancellationToken);

        return result.IsSuccess
            ? Result.Success(result.Value.ToIExternalWorkItemLinks())
            : Result.Failure<List<IExternalWorkItemLink>>(result.Error);
    }

    public async Task<Result<List<IExternalWorkItemLink>>> GetDependencyLinkChanges(string organizationUrl, string token, string projectName, DateTime lastChangedDate, string[] workItemTypes, CancellationToken cancellationToken)
    {
        var workItemService = GetService<WorkItemService>(organizationUrl, token);

        var result = await workItemService.GetDependencyLinkChanges(projectName, lastChangedDate, workItemTypes, cancellationToken);

        return result.IsSuccess
            ? Result.Success(result.Value.ToIExternalWorkItemLinks())
            : Result.Failure<List<IExternalWorkItemLink>>(result.Error);
    }

    public async Task<Result<int[]>> GetDeletedWorkItemIds(string organizationUrl, string token, string projectName, DateTime lastChangedDate, CancellationToken cancellationToken)
    {
        var workItemService = GetService<WorkItemService>(organizationUrl, token);

        var result = await workItemService.GetDeletedWorkItemIds(projectName, lastChangedDate, cancellationToken);

        return result.IsSuccess
            ? Result.Success(result.Value)
            : Result.Failure<int[]>(result.Error);
    }

    private TService GetService<TService>(string organizationUrl, string token)
    {
        Guard.Against.NullOrWhiteSpace(organizationUrl, nameof(organizationUrl));
        Guard.Against.NullOrWhiteSpace(token, nameof(token));
        var logger = _serviceProvider.GetService(typeof(ILogger<TService>)) as ILogger<TService>;
        Guard.Against.Null(logger);

        return typeof(TService) switch
        {
            Type type when type == typeof(ProcessService) => (TService)Activator.CreateInstance(typeof(ProcessService), organizationUrl, token, _apiVersion, logger!)!,
            Type type when type == typeof(ProjectService) => (TService)Activator.CreateInstance(typeof(ProjectService), organizationUrl, token, _apiVersion, logger!)!,
            Type type when type == typeof(WorkItemService) => (TService)Activator.CreateInstance(typeof(WorkItemService), organizationUrl, token, _apiVersion, logger!)!,
            _ => throw new NotImplementedException(),
        };
    }
}
