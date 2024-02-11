using Mapster;
using Microsoft.EntityFrameworkCore;
using Moda.Work.Application.WorkStatuses.Dtos;

namespace Moda.Work.Application.WorkStatuses.Queries;
public sealed record GetWorkStatusesQuery(bool IncludeInactive = false) : IQuery<IReadOnlyList<WorkStatusDto>>;

internal sealed class GetWorkStatusesQueryHandler : IQueryHandler<GetWorkStatusesQuery, IReadOnlyList<WorkStatusDto>>
{
    private readonly IWorkDbContext _workDbContext;

    public GetWorkStatusesQueryHandler(IWorkDbContext workDbContext)
    {
        _workDbContext = workDbContext;
    }

    public async Task<IReadOnlyList<WorkStatusDto>> Handle(GetWorkStatusesQuery request, CancellationToken cancellationToken)
    {
        var query = _workDbContext.WorkStatuses.AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(e => e.IsActive);

        return await query.ProjectToType<WorkStatusDto>().ToListAsync(cancellationToken);
    }
}
