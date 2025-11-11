using System.Linq.Expressions;
using Moda.Work.Application.Persistence;
using Moda.Work.Application.WorkItems.Dtos;
using Moda.Work.Application.Workspaces.Models;

namespace Moda.Work.Application.WorkItems.Queries;
public sealed record GetWorkItemDependenciesQuery : IQuery<Result<List<ScopedDependencyDto>?>>
{
    public GetWorkItemDependenciesQuery(WorkspaceIdOrKey workspaceIdOrKey, WorkItemKey workItemKey)
    {
        WorkspaceIdOrKeyFilter = workspaceIdOrKey.CreateWorkspaceFilter<WorkItem>();
        //WorkspaceIdOrKeyFilter = workspaceIdOrKey.CreateFilter<WorkItem>(wi => wi.WorkspaceId, wi => wi.Workspace.Key);
        WorkItemKey = workItemKey;
    }

    public Expression<Func<WorkItem, bool>> WorkspaceIdOrKeyFilter { get; }
    public WorkItemKey WorkItemKey { get; }
}

internal sealed class GetWorkItemDependenciesQueryHandler(IWorkDbContext workDbContext, ILogger<GetWorkItemDependenciesQueryHandler> logger) : IQueryHandler<GetWorkItemDependenciesQuery, Result<List<ScopedDependencyDto>?>>
{
    private const string AppRequestName = nameof(GetWorkItemDependenciesQuery);

    private readonly IWorkDbContext _workDbContext = workDbContext;
    private readonly ILogger<GetWorkItemDependenciesQueryHandler> _logger = logger;

    public async Task<Result<List<ScopedDependencyDto>?>> Handle(GetWorkItemDependenciesQuery request, CancellationToken cancellationToken)
    {
        var workItemId = await _workDbContext.WorkItems
            .Where(w => w.Key == request.WorkItemKey)
            .Where(request.WorkspaceIdOrKeyFilter)
            .Select(w => (Guid?)w.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!workItemId.HasValue)
        {
            _logger.LogInformation("{AppRequestName} - Work item with key {WorkItemKey} not found", AppRequestName, request.WorkItemKey);
            return null;
        }

        var dependencyLinks = await _workDbContext.WorkItemDependencies
            .Where(l => l.RemovedOn == null)
            .Where(l => l.SourceId == workItemId || l.TargetId == workItemId)
            .ProjectToType<DependencyDto>()
            .ToListAsync(cancellationToken);

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("{AppRequestName} - Found {DependencyLinksCount} dependency links for work item {WorkItemKey}", AppRequestName, dependencyLinks.Count, request.WorkItemKey);

        return dependencyLinks.Count == 0
            ? []
            : dependencyLinks
                .Select(d => ScopedDependencyDto.From(d, request.WorkItemKey))
                .ToList();
    }
}
