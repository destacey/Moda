using Moda.Work.Application.WorkTypeLevels.Dtos;

namespace Moda.Work.Application.WorkTypeLevels.Queries;
public sealed record GetWorkTypeLevelsQuery() : IQuery<IReadOnlyList<WorkTypeLevelDto>>;

internal sealed class GetWorkTypeLevelsQueryHandler : IQueryHandler<GetWorkTypeLevelsQuery, IReadOnlyList<WorkTypeLevelDto>>
{
    private readonly IWorkDbContext _workDbContext;

    public GetWorkTypeLevelsQueryHandler(IWorkDbContext workDbContext)
    {
        _workDbContext = workDbContext;
    }

    public async Task<IReadOnlyList<WorkTypeLevelDto>> Handle(GetWorkTypeLevelsQuery request, CancellationToken cancellationToken)
    {
        return await _workDbContext.WorkTypeHierarchies
            .SelectMany(s => s.Levels)
            .ProjectToType<WorkTypeLevelDto>()
            .ToListAsync(cancellationToken);
    }
}
