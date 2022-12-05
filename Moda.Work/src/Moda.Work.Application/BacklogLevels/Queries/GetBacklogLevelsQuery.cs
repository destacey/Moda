using Mapster;
using Microsoft.EntityFrameworkCore;
using Moda.Work.Application.BacklogLevels.Dtos;

namespace Moda.Work.Application.BacklogLevels.Queries;
public sealed record GetBacklogLevelsQuery : IQuery<IReadOnlyList<BacklogLevelDto>>
{
    public GetBacklogLevelsQuery(bool includeInactive = false)
    {
        IncludeInactive = includeInactive;
    }

    public bool IncludeInactive { get; }
}

internal sealed class GetBacklogLevelsQueryHandler : IQueryHandler<GetBacklogLevelsQuery, IReadOnlyList<BacklogLevelDto>>
{
    private readonly IWorkDbContext _workDbContext;

    public GetBacklogLevelsQueryHandler(IWorkDbContext workDbContext)
    {
        _workDbContext = workDbContext;
    }

    public async Task<IReadOnlyList<BacklogLevelDto>> Handle(GetBacklogLevelsQuery request, CancellationToken cancellationToken)
    {
        var query = _workDbContext.BacklogLevels.AsQueryable();
        
        if (!request.IncludeInactive)
            query = query.Where(b => b.IsActive);

        return await query.ProjectToType<BacklogLevelDto>().ToListAsync(cancellationToken);
    }
}
