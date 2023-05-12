using Moda.Planning.Application.Risks.Dtos;

namespace Moda.Planning.Application.Risks.Queries;
public sealed record GetRisksQuery : IQuery<IReadOnlyList<RiskListDto>>
{
    public GetRisksQuery() {}

    public GetRisksQuery(Guid teamId)
    {
        TeamIds = new List<Guid> { teamId };
    }

    public GetRisksQuery(IEnumerable<Guid> teamIds)
    {
        TeamIds = teamIds.ToList();
    }

    public List<Guid> TeamIds { get; set; } = new();
}

internal sealed class GetRisksQueryHandler : IQueryHandler<GetRisksQuery, IReadOnlyList<RiskListDto>>
{
    private readonly IPlanningDbContext _planningDbContext;

    public GetRisksQueryHandler(IPlanningDbContext planningDbContext)
    {
        _planningDbContext = planningDbContext;
    }

    public async Task<IReadOnlyList<RiskListDto>> Handle(GetRisksQuery request, CancellationToken cancellationToken)
    {
        var query = _planningDbContext.Risks
            .Include(r => r.Team)
            .Include(r => r.ReportedBy)
            .Include(r => r.Assignee)
            .AsQueryable();

        if (request.TeamIds.Any())
        {
            query = query.Where(r => r.TeamId.HasValue && request.TeamIds.Contains(r.TeamId.Value));
        }

        var risks = await query
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return risks.Adapt<IReadOnlyList<RiskListDto>>();
    }
}
