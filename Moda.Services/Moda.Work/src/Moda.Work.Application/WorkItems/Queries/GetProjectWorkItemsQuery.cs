using Moda.Work.Application.Persistence;
using Moda.Work.Application.WorkItems.Dtos;

namespace Moda.Work.Application.WorkItems.Queries;

// TODO: change this to IdOrKey parameter
public sealed record GetProjectWorkItemsQuery(Guid ProjectId) : IQuery<Result<List<WorkItemListDto>>>;

internal sealed class GetProjectWorkItemsQueryHandler(IWorkDbContext workDbContext)
    : IQueryHandler<GetProjectWorkItemsQuery, Result<List<WorkItemListDto>>>
{
    private readonly IWorkDbContext _workDbContext = workDbContext;

    public async Task<Result<List<WorkItemListDto>>> Handle(GetProjectWorkItemsQuery request, CancellationToken cancellationToken)
    {
        return await _workDbContext.WorkItems
            .Where(i => (i.ProjectId != null && i.ProjectId == request.ProjectId) 
                || (i.ProjectId == null && i.ParentProjectId != null && i.ParentProjectId == request.ProjectId))
            .ProjectToType<WorkItemListDto>()
            .ToListAsync(cancellationToken);
    }
}
