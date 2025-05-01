using Moda.Work.Application.Persistence;
using Moda.Work.Application.Workspaces.Dtos;

namespace Moda.Work.Application.Workspaces.Queries;
public sealed record GetWorkspacesQuery(bool IncludeInactive = false) : IQuery<IReadOnlyList<WorkspaceListDto>>;

internal sealed class GetWorkspacesQueryHandler(IWorkDbContext workDbContext) : IQueryHandler<GetWorkspacesQuery, IReadOnlyList<WorkspaceListDto>>
{
    private readonly IWorkDbContext _workDbContext = workDbContext;

    public async Task<IReadOnlyList<WorkspaceListDto>> Handle(GetWorkspacesQuery request, CancellationToken cancellationToken)
    {
        var query = _workDbContext.Workspaces.AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(e => e.IsActive);

        return await query.ProjectToType<WorkspaceListDto>().ToListAsync(cancellationToken);
    }
}
