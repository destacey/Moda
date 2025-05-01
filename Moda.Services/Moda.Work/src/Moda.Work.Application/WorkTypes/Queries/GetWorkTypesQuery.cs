using Moda.Work.Application.Persistence;
using Moda.Work.Application.WorkTypes.Dtos;

namespace Moda.Work.Application.WorkTypes.Queries;
public sealed record GetWorkTypesQuery : IQuery<IReadOnlyList<WorkTypeDto>>
{
    public GetWorkTypesQuery(bool includeInactive = false)
    {
        IncludeInactive = includeInactive;
    }

    public bool IncludeInactive { get; }
}

internal sealed class GetWorkTypesQueryHandler : IQueryHandler<GetWorkTypesQuery, IReadOnlyList<WorkTypeDto>>
{
    private readonly IWorkDbContext _workDbContext;

    public GetWorkTypesQueryHandler(IWorkDbContext workDbContext)
    {
        _workDbContext = workDbContext;
    }

    public async Task<IReadOnlyList<WorkTypeDto>> Handle(GetWorkTypesQuery request, CancellationToken cancellationToken)
    {
        var query = _workDbContext.WorkTypes.AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(e => e.IsActive);

        return await query.ProjectToType<WorkTypeDto>().ToListAsync(cancellationToken);
    }
}
