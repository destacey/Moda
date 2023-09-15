using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Moda.Work.Application.BacklogLevels.Queries;
public sealed record GetBacklogLevelsQuery : IQuery<IReadOnlyList<BacklogLevelDto>>
{
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
        return await _workDbContext.BacklogLevelSchemes
            .SelectMany(s => s.BacklogLevels)
            .ProjectToType<BacklogLevelDto>()
            .ToListAsync(cancellationToken);
    }
}
