using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moda.Common.Application.Interfaces;
using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Models;
using Moda.Integrations.AzureDevOps.Models.Projects;
using Moda.Integrations.AzureDevOps.Models.WorkItems;
using Moda.Integrations.AzureDevOps.Services;
using Moda.Integrations.AzureDevOps.Utils;

namespace Moda.Integrations.AzureDevOps;

public class AzureDevOpsService(ILogger<AzureDevOpsService> logger, IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider, IMemoryCache memoryCache) : IAzureDevOpsService
{
    // https://learn.microsoft.com/en-us/azure/devops/integrate/concepts/rest-api-versioning?view=azure-devops#supported-versions
    private readonly string _apiVersion = "7.0";

    private readonly ILogger<AzureDevOpsService> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly IMemoryCache _memoryCache = memoryCache;

    public async Task<Result> TestConnection(string organizationUrl, string token)
    {
        try
        {
            // use the GetInstanceId method to test the connection
            var result = await GetSystemId(organizationUrl, token, CancellationToken.None).ConfigureAwait(false);

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

    public async Task<Result<string>> GetSystemId(string organizationUrl, string token, CancellationToken cancellationToken)
    {
        var generalService = GetService<GeneralService>(organizationUrl, token);
        
        var result = await generalService.GetConnectionData(cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
            return Result.Failure<string>(result.Error);

        return result.Value is not null
            ? result.Value.InstanceId
            : Result.Failure<string>("No systemId returned.");
    }

    public async Task<Result<List<IExternalWorkProcess>>> GetWorkProcesses(string organizationUrl, string token, CancellationToken cancellationToken)
    {
        var processService = GetService<ProcessService>(organizationUrl, token);

        var result = await processService.GetProcesses(cancellationToken).ConfigureAwait(false);

        return result.IsSuccess
            ? result.Value.ToList<IExternalWorkProcess>()
            : Result.Failure<List<IExternalWorkProcess>>(result.Error);
    }

    public async Task<Result<IExternalWorkProcessConfiguration>> GetWorkProcess(string organizationUrl, string token, Guid processId, CancellationToken cancellationToken)
    {
        var processService = GetService<ProcessService>(organizationUrl, token);

        var result = await processService.GetProcess(processId, cancellationToken).ConfigureAwait(false);

        return result.IsSuccess
            ? result.Value
            : Result.Failure<IExternalWorkProcessConfiguration>(result.Error);
    }

    public async Task<Result<IExternalWorkspaceConfiguration>> GetWorkspace(string organizationUrl, string token, Guid workspaceId, CancellationToken cancellationToken)
    {
        var result = await GetProject(organizationUrl, token, workspaceId.ToString(), cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
            return Result.Failure<IExternalWorkspaceConfiguration>(result.Error);

        if (!result.Value.HasProcessTemplateType)
            return Result.Failure<IExternalWorkspaceConfiguration>("Workspace does not have a process template type.");

        return result.Value.ToAzdoWorkspaceConfiguration();
    }

    public async Task<Result<List<IExternalWorkspace>>> GetWorkspaces(string organizationUrl, string token, CancellationToken cancellationToken)
    {
        var projectService = GetService<ProjectService>(organizationUrl, token);

        var result = await projectService.GetProjects(cancellationToken).ConfigureAwait(false);

        return result.IsSuccess
            ? result.Value.ToAzdoWorkspaces().ToList<IExternalWorkspace>()
            : Result.Failure<List<IExternalWorkspace>>(result.Error);
    }

    public async Task<Result<List<IExternalTeam>>> GetTeams(string organizationUrl, string token, Guid[] projectIds, CancellationToken cancellationToken)
    {
        var projectService = GetService<ProjectService>(organizationUrl, token);

        var result = await projectService.GetTeams(projectIds, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
            return Result.Failure<List<IExternalTeam>>(result.Error);

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("{TeamCount} teams found for organization {organizationUrl}.", result.Value.Count, organizationUrl);

        return result.Value;
    }

    public async Task<Result<List<IExternalIteration<AzdoIterationMetadata>>>> GetIterations(string organizationUrl, string token, string projectName, Dictionary<Guid, Guid?> teamSettings, CancellationToken cancellationToken)
    {
        var projectResult = await GetProject(organizationUrl, token, projectName, cancellationToken).ConfigureAwait(false);
        if (projectResult.IsFailure)
            return Result.Failure<List<IExternalIteration<AzdoIterationMetadata>>>($"Unable to get details for project {projectName}");

        var iterationsResult = await GetOrFetchIterationsAsync(organizationUrl, token, projectName, teamSettings, cancellationToken).ConfigureAwait(false);

        return iterationsResult.IsSuccess
            ? iterationsResult.Value.ToIExternalIterations(_dateTimeProvider.Now, projectResult.Value.Id)
            : Result.Failure<List<IExternalIteration<AzdoIterationMetadata>>>(iterationsResult.Error);
    }

    public async Task<Result<List<IExternalWorkItem>>> GetWorkItems(string organizationUrl, string token, string projectName, DateTime lastChangedDate, string[] workItemTypes, Dictionary<Guid, Guid?> teamSettings, CancellationToken cancellationToken)
    {
        var iterationsResult = await GetOrFetchIterationsAsync(organizationUrl, token, projectName, teamSettings, cancellationToken).ConfigureAwait(false);
        if (iterationsResult.IsFailure)
            return Result.Failure<List<IExternalWorkItem>>(iterationsResult.Error);

        var cachedIterations = iterationsResult.Value;

        var workItemService = GetService<WorkItemService>(organizationUrl, token);

        var result = await workItemService.GetWorkItems(projectName, lastChangedDate, workItemTypes, cancellationToken).ConfigureAwait(false);

        return result.IsSuccess
            ? result.Value.ToIExternalWorkItems(cachedIterations)
            : Result.Failure<List<IExternalWorkItem>>(result.Error);
    }

    public async Task<Result<List<IExternalWorkItemLink>>> GetParentLinkChanges(string organizationUrl, string token, string projectName, DateTime lastChangedDate, string[] workItemTypes, CancellationToken cancellationToken)
    {
        var workItemService = GetService<WorkItemService>(organizationUrl, token);

        var result = await workItemService.GetParentLinkChanges(projectName, lastChangedDate, workItemTypes, cancellationToken).ConfigureAwait(false);

        return result.IsSuccess
            ? result.Value.ToIExternalWorkItemLinks()
            : Result.Failure<List<IExternalWorkItemLink>>(result.Error);
    }

    public async Task<Result<List<IExternalWorkItemLink>>> GetDependencyLinkChanges(string organizationUrl, string token, string projectName, DateTime lastChangedDate, string[] workItemTypes, CancellationToken cancellationToken)
    {
        var workItemService = GetService<WorkItemService>(organizationUrl, token);

        var result = await workItemService.GetDependencyLinkChanges(projectName, lastChangedDate, workItemTypes, cancellationToken).ConfigureAwait(false);

        return result.IsSuccess
            ? result.Value.ToIExternalWorkItemLinks()
            : Result.Failure<List<IExternalWorkItemLink>>(result.Error);
    }

    public async Task<Result<int[]>> GetDeletedWorkItemIds(string organizationUrl, string token, string projectName, DateTime lastChangedDate, CancellationToken cancellationToken)
    {
        var workItemService = GetService<WorkItemService>(organizationUrl, token);

        var result = await workItemService.GetDeletedWorkItemIds(projectName, lastChangedDate, cancellationToken).ConfigureAwait(false);

        return result.IsSuccess
            ? result.Value
            : Result.Failure<int[]>(result.Error);
    }

    private async Task<Result<ProjectDetailsDto>> GetProject(string organizationUrl, string token, string projectIdOrName, CancellationToken cancellationToken)
    {
        var projectService = GetService<ProjectService>(organizationUrl, token);

        return await projectService.GetProject(projectIdOrName, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Result<List<IterationDto>>> GetOrFetchIterationsAsync(string organizationUrl, string token, string projectName, Dictionary<Guid, Guid?>? teamSettings, CancellationToken cancellationToken, bool forceRefresh = false)
    {
        var cacheKey = CacheKeyGenerator.GetCacheKey("azdo-iterations", organizationUrl, projectName, teamSettings);

        if (!forceRefresh && _memoryCache.TryGetValue(cacheKey, out List<IterationDto>? cached) && cached is not null)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Returning cached iterations for {cacheKey}", cacheKey);

            return Result.Success(cached);
        }

        var projectService = GetService<ProjectService>(organizationUrl, token);
        var result = await projectService.GetIterations(projectName, teamSettings, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
            return Result.Failure<List<IterationDto>>(result.Error);

        // ensure we never cache null; allow empty list
        var toCache = result.Value ?? [];
        var iterationCacheOptions = new MemoryCacheEntryOptions
        {
            // the goal is to have a short-lived cache for a single sync run
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
        };

        _memoryCache.Set(cacheKey, toCache, iterationCacheOptions);

        return Result.Success(toCache);
    }

    private TService GetService<TService>(string organizationUrl, string token)
    {
        Guard.Against.NullOrWhiteSpace(organizationUrl, nameof(organizationUrl));
        Guard.Against.NullOrWhiteSpace(token, nameof(token));
        var logger = _serviceProvider.GetService(typeof(ILogger<TService>)) as ILogger<TService>;
        Guard.Against.Null(logger);

        return typeof(TService) switch
        {
            Type type when type == typeof(ProcessService) => (TService)Activator.CreateInstance(typeof(ProcessService), organizationUrl, token, _apiVersion, logger)!,
            Type type when type == typeof(ProjectService) => (TService)Activator.CreateInstance(typeof(ProjectService), organizationUrl, token, _apiVersion, logger)!,
            Type type when type == typeof(WorkItemService) => (TService)Activator.CreateInstance(typeof(WorkItemService), organizationUrl, token, _apiVersion, logger)!,
            Type type when type == typeof(GeneralService) => (TService)Activator.CreateInstance(typeof(GeneralService), organizationUrl, token, _apiVersion, logger)!,
            _ => throw new NotImplementedException(),
        };
    }
}