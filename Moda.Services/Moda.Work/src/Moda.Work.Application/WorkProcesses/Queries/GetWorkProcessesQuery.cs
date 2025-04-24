using Moda.Work.Application.Persistence;
using Moda.Work.Application.WorkProcesses.Dtos;

namespace Moda.Work.Application.WorkProcesses.Queries;
public sealed record GetWorkProcessesQuery(bool IncludeInactive = false) : IQuery<IReadOnlyList<WorkProcessListDto>>;

internal sealed class GetWorkProcessesQueryHandler(IWorkDbContext workDbContext, ILogger<GetWorkProcessesQueryHandler> logger) : IQueryHandler<GetWorkProcessesQuery, IReadOnlyList<WorkProcessListDto>>
{
    private readonly IWorkDbContext _workDbContext = workDbContext;
    private readonly ILogger<GetWorkProcessesQueryHandler> _logger = logger;

    public async Task<IReadOnlyList<WorkProcessListDto>> Handle(GetWorkProcessesQuery request, CancellationToken cancellationToken)
    {
        var query = _workDbContext.WorkProcesses.AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(e => e.IsActive);

        return await query.ProjectToType<WorkProcessListDto>().ToListAsync(cancellationToken);
    }
}
