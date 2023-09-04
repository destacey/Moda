using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Moda.Common.Application.Interfaces;
using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Interfaces.Work;
using Moda.Integrations.AzureDevOps.Models;
using Moda.Integrations.AzureDevOps.Services;

namespace Moda.Integrations.AzureDevOps;

public class AzureDevOpsService : IAzureDevOpsService
{
    private readonly ILogger<AzureDevOpsService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AzureDevOpsService(ILogger<AzureDevOpsService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<Result> TestConnection(string organizationUrl, string token)
    {
        try
        {
            var connection = CreateVssConnection(organizationUrl, token);
            var projectService = GetService<ProjectService>(connection);

            return await projectService.GetProjects();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Azure DevOps connection.");
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<List<WorkItemClassificationNode>>> GetAreas(string organizationUrl, string token, Guid projectId, CancellationToken cancellationToken)
    {
        var connection = CreateVssConnection(organizationUrl, token);
        var areaService = GetService<AreaService>(connection);

        var areas = await areaService.GetAreas(projectId, cancellationToken);

        return areas; // map to local type and return interface instead of concrete type
    }

    public async Task<Result<List<WorkItemClassificationNode>>> GetIterations(string organizationUrl, string token, Guid projectId, CancellationToken cancellationToken)
    {
        var connection = CreateVssConnection(organizationUrl, token);
        var iterationService = GetService<IterationService>(connection);

        var iterations = await iterationService.GetIterations(projectId, cancellationToken);

        return iterations; // map to local type and return interface instead of concrete type
    }

    public async Task<Result<IExternalWorkspace>> GetProject(string organizationUrl, string token, Guid projectId)
    {
        var connection = CreateVssConnection(organizationUrl, token);
        var projectService = GetService<ProjectService>(connection);

        var result = await projectService.GetProject(projectId);

        return result.IsSuccess
            ? Result.Success<IExternalWorkspace>(new AzdoWorkspace(result.Value))
            : Result.Failure<IExternalWorkspace>(result.Error);
    }

    public async Task<Result<List<IExternalWorkspace>>> GetProjects(string organizationUrl, string token)
    {
        var connection = CreateVssConnection(organizationUrl, token);
        var projectService = GetService<ProjectService>(connection);

        var result = await projectService.GetProjects();
        if (result.IsFailure)
            return Result.Failure<List<IExternalWorkspace>>(result.Error);

        var workspaces = result.Value
            .Select(w => new AzdoWorkspace(w))
            .ToList<IExternalWorkspace>();

        return Result.Success(workspaces);
    }

    public async Task<Result<IExternalWorkItem>> GetWorkItem(string organizationUrl, string token, Guid projectId, int workItemId, CancellationToken cancellationToken)
    {
        var connection = CreateVssConnection(organizationUrl, token);
        var workItemService = GetService<WorkItemService>(connection);

        var result = await workItemService.GetWorkItem(projectId, workItemId, cancellationToken);

        return result.IsSuccess
            ? Result.Success<IExternalWorkItem>(new AzdoWorkItem(result.Value))
            : Result.Failure<IExternalWorkItem>(result.Error);
    }

    public async Task<Result<List<IExternalWorkItem>>> GetWorkItems(string organizationUrl, string token, Guid projectId, string projectName, CancellationToken cancellationToken)
    {
        var connection = CreateVssConnection(organizationUrl, token);
        var workItemService = GetService<WorkItemService>(connection);

        var result = await workItemService.GetWorkItems(projectId, projectName, cancellationToken);
        if (result.IsFailure)
            return Result.Failure<List<IExternalWorkItem>>(result.Error);

        var workItems = result.Value
            .Select(w => new AzdoWorkItem(w))
            .ToList<IExternalWorkItem>();

        return Result.Success(workItems);
    }

    public async Task<Result<List<IExternalWorkItem>>> GetWorkItems(string organizationUrl, string token, Guid projectId, int[] workItemIds, CancellationToken cancellationToken)
    {
        var connection = CreateVssConnection(organizationUrl, token);
        var workItemService = GetService<WorkItemService>(connection);

        var result = await workItemService.GetWorkItems(projectId, workItemIds, cancellationToken);
        if (result.IsFailure)
            return Result.Failure<List<IExternalWorkItem>>(result.Error);

        var workItems = result.Value
            .Select(w => new AzdoWorkItem(w))
            .ToList<IExternalWorkItem>();

        return Result.Success(workItems);
    }

    public async Task<Result<List<IExternalWorkType>>> GetWorkItemTypes(string organizationUrl, string token, Guid projectId, CancellationToken cancellationToken)
    {
        var connection = CreateVssConnection(organizationUrl, token);
        var workItemTypeService = GetService<WorkItemTypeService>(connection);

        var result = await workItemTypeService.GetWorkItemTypes(projectId, cancellationToken);
        if (result.IsFailure)
            return Result.Failure<List<IExternalWorkType>>(result.Error);

        var workItems = result.Value
            .Select(w => new AzdoWorkType(w))
            .ToList<IExternalWorkType>();

        return Result.Success(workItems);
    }

    // TODO should these be cached?  any impact on GC if cached?  // should the client be created and cached here rather than in the service constructor?
    private TService GetService<TService>(VssConnection connection)
    {
        Guard.Against.Null(connection);
        var logger = _serviceProvider.GetService(typeof(ILogger<TService>)) as ILogger<TService>;
        Guard.Against.Null(logger);

        return typeof(TService) switch
        {
            Type type when type == typeof(AreaService) => (TService)Activator.CreateInstance(typeof(AreaService), connection, logger!)!,
            Type type when type == typeof(IterationService) => (TService)Activator.CreateInstance(typeof(IterationService), connection, logger!)!,
            Type type when type == typeof(ProjectService) => (TService)Activator.CreateInstance(typeof(ProjectService), connection, logger!)!,
            Type type when type == typeof(WorkItemService) => (TService)Activator.CreateInstance(typeof(WorkItemService), connection, logger!)!,
            Type type when type == typeof(WorkItemTypeService) => (TService)Activator.CreateInstance(typeof(WorkItemTypeService), connection, logger!)!,
            _ => throw new NotImplementedException(),
        };
    }

    private static VssConnection CreateVssConnection(string organizationUrl, string token)
    {
        // connect to Azure DevOps Services
        VssBasicCredential credential = new(string.Empty, Guard.Against.NullOrWhiteSpace(token));
        return new VssConnection(new Uri(Guard.Against.NullOrWhiteSpace(organizationUrl)), credential);
    }
}
