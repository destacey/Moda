using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Moda.Integrations.AzureDevOps.Services;

namespace Moda.Integrations.AzureDevOps;

public class AzureDevOpsService  // TODO add interface
{
    private readonly VssConnection _connection;
    private readonly ILogger<AzureDevOpsService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AzureDevOpsService(string organizationUrl, string token, ILogger<AzureDevOpsService> logger, IServiceProvider serviceProvider)
    {
        // connect to Azure DevOps Services
        VssBasicCredential credential = new(string.Empty, Guard.Against.NullOrWhiteSpace(token));
        _connection = new VssConnection(new Uri(Guard.Against.NullOrWhiteSpace(organizationUrl)), credential);

        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<List<WorkItemClassificationNode>>> GetAreas(Guid projectId, CancellationToken cancellationToken)
    {
        var areaService = GetService<AreaService>();

        var areas = await areaService.GetAreas(projectId, cancellationToken);

        return areas; // map to local type and return interface instead of concrete type
    }
    
    public async Task<Result<List<WorkItemClassificationNode>>> GetIterations(Guid projectId, CancellationToken cancellationToken)
    {
        var iterationService = GetService<IterationService>();

        var iterations = await iterationService.GetIterations(projectId, cancellationToken);

        return iterations; // map to local type and return interface instead of concrete type
    }

    public async Task<Result<TeamProject>> GetProject(Guid projectId)
    {
        var projectService = GetService<ProjectService>();

        var project = await projectService.GetProject(projectId);
        
        return project; // map to local type and return interface instead of concrete type
    }

    public async Task<Result<List<TeamProjectReference>>> GetProjects()
    {
        var projectService = GetService<ProjectService>();

        var projects = await projectService.GetProjects();

        return projects; // map to local type and return interface instead of concrete type
    }

    public async Task<Result<WorkItem>> GetWorkItem(Guid projectId, int workItemId, CancellationToken cancellationToken)
    {
        var workItemService = GetService<WorkItemService>();

        var result = await workItemService.GetWorkItem(projectId, workItemId, cancellationToken);

        return result; // map to local type and return interface instead of concrete type
    }

    public async Task<Result<List<WorkItem>>> GetWorkItems(Guid projectId, string projectName, CancellationToken cancellationToken)
    {
        var workItemService = GetService<WorkItemService>();

        var result = await workItemService.GetWorkItems(projectId, projectName, cancellationToken);

        return result; // map to local type and return interface instead of concrete type
    }

    public async Task<Result<List<WorkItem>>> GetWorkItems(Guid projectId, int[] workItemIds, CancellationToken cancellationToken)
    {
        var workItemService = GetService<WorkItemService>();

        var workItems = await workItemService.GetWorkItems(projectId, workItemIds, cancellationToken);

        return workItems; // map to local type and return interface instead of concrete type
    }

    public async Task<Result<List<WorkItemType>>> GetWorkItemTypes(Guid projectId, CancellationToken cancellationToken)
    {
        var workItemTypeService = GetService<WorkItemTypeService>();

        var types = await workItemTypeService.GetWorkItemTypes(projectId, cancellationToken);

        return types; // map to local type and return interface instead of concrete type
    }

    // TODO should these be cached?  any impact on GC if cached?  // should the client be created and cached here rather than in the service constructor?
    private TService GetService<TService>()
    {
        var logger = _serviceProvider.GetService(typeof(ILogger<TService>)) as ILogger<TService>;
        Guard.Against.Null(logger);

        return typeof(TService) switch
        {
            Type type when type == typeof(AreaService) => (TService)Activator.CreateInstance(typeof(AreaService), _connection, logger!)!,
            Type type when type == typeof(IterationService) => (TService)Activator.CreateInstance(typeof(IterationService), _connection, logger!)!,
            Type type when type == typeof(ProjectService) => (TService)Activator.CreateInstance(typeof(ProjectService), _connection, logger!)!,
            Type type when type == typeof(WorkItemService) => (TService)Activator.CreateInstance(typeof(WorkItemService), _connection, logger!)!,
            Type type when type == typeof(WorkItemTypeService) => (TService)Activator.CreateInstance(typeof(WorkItemTypeService), _connection, logger!)!,
            _ => throw new NotImplementedException(),
        };
    }
}
