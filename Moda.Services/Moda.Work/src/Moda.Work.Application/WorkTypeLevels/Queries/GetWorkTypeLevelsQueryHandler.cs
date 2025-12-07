using Moda.Common.Application.Requests.WorkManagement.Interfaces;
using Moda.Common.Application.Requests.WorkManagement.Queries;
using Moda.Work.Application.Persistence;

namespace Moda.Work.Application.WorkTypeLevels.Queries;

internal sealed class GetWorkTypeLevelsQueryHandler : IQueryHandler<GetWorkTypeLevelsQuery, IReadOnlyList<IWorkTypeLevelDto>>
{
    private readonly IWorkDbContext _workDbContext;

    public GetWorkTypeLevelsQueryHandler(IWorkDbContext workDbContext)
    {
        _workDbContext = workDbContext;
    }

    public async Task<IReadOnlyList<IWorkTypeLevelDto>> Handle(GetWorkTypeLevelsQuery request, CancellationToken cancellationToken)
    {
        return await _workDbContext.WorkTypeHierarchies
            .SelectMany(s => s.Levels)
            .ProjectToType<WorkTypeLevelDto>()
            .ToListAsync(cancellationToken);
    }
}
