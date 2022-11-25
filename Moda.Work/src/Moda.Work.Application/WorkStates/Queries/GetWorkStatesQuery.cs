using Mapster;
using Microsoft.EntityFrameworkCore;
using Moda.Work.Application.WorkStates.Dtos;

namespace Moda.Work.Application.WorkStates.Queries;
public sealed record GetWorkStatesQuery : IQuery<IReadOnlyList<WorkStateListDto>>
{
    public GetWorkStatesQuery(bool includeInactive = false)
    {
        IncludeInactive = includeInactive;
    }

    public bool IncludeInactive { get; }
}

internal sealed class GetEmployeesQueryHandler : IQueryHandler<GetWorkStatesQuery, IReadOnlyList<WorkStateListDto>>
{
    private readonly IWorkDbContext _workDbContext;

    public GetEmployeesQueryHandler(IWorkDbContext workDbContext)
    {
        _workDbContext = workDbContext;
    }

    public async Task<IReadOnlyList<WorkStateListDto>> Handle(GetWorkStatesQuery request, CancellationToken cancellationToken)
    {
        var query = _workDbContext.WorkStates.AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(e => e.IsActive);

        return await query.ProjectToType<WorkStateListDto>().ToListAsync(cancellationToken);
    }
}
