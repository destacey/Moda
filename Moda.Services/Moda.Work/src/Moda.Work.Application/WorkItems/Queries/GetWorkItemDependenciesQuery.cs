using System.Linq.Expressions;
using Moda.Common.Domain.Enums.Work;
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
        var exists = await _workDbContext.WorkItems
            .Where(w => w.Key == request.WorkItemKey)
            .Where(request.WorkspaceIdOrKeyFilter)
            .AnyAsync(cancellationToken);

        if (!exists)
        {
            _logger.LogInformation("{AppRequestName} - Work item with key {WorkItemKey} not found", AppRequestName, request.WorkItemKey);
            return null;
        }

        var dependencyLinks = await _workDbContext.WorkItemLinks
            .Where(l => l.LinkType == WorkItemLinkType.Dependency)
            .Where(l => l.RemovedOn == null)  // Only get active links
            .Where(l => l.Source!.Key == request.WorkItemKey || l.Target!.Key == request.WorkItemKey)
            .ProjectToType<WorkItemLinkDto>()
            .ToListAsync(cancellationToken);

        _logger.LogDebug("{AppRequestName} - Found {DependencyLinksCount} dependency links for work item {WorkItemKey}", AppRequestName, dependencyLinks.Count, request.WorkItemKey);

        return dependencyLinks.Count == 0
            ? []
            : dependencyLinks
                .Select(d => ScopedDependencyDto.From(d, request.WorkItemKey))
                .ToList();
    }
}
