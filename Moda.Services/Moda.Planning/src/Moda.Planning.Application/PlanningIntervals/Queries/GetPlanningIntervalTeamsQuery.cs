namespace Moda.Planning.Application.PlanningIntervals.Queries;

public sealed record GetPlanningIntervalTeamsQuery(Guid Id) : IQuery<IReadOnlyList<Guid>>;

internal sealed class GetPlanningIntervalTeamsQueryHandler : IQueryHandler<GetPlanningIntervalTeamsQuery, IReadOnlyList<Guid>>
{
    private readonly IPlanningDbContext _planningDbContext;

    public GetPlanningIntervalTeamsQueryHandler(IPlanningDbContext planningDbContext)
    {
        _planningDbContext = planningDbContext;
    }

    public async Task<IReadOnlyList<Guid>> Handle(GetPlanningIntervalTeamsQuery request, CancellationToken cancellationToken)
    {
        return await _planningDbContext.PlanningIntervals
            .Where(p => p.Id == request.Id)
            .SelectMany(p => p.Teams.Select(t => t.TeamId))
            .ToListAsync(cancellationToken);
    }
}
