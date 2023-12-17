using Moda.Goals.Application.Objectives.Dtos;
using Moda.Goals.Application.Persistence;

namespace Moda.Goals.Application.Objectives.Queries;
public sealed record GetObjectivesForPlanningIntervalsQuery : IQuery<IReadOnlyList<ObjectiveListDto>>
{
    public GetObjectivesForPlanningIntervalsQuery(Guid[] planningIntervalIds, Guid[]? teamIds)
    {
        PlanningIntervalIds = planningIntervalIds;
        TeamIds = teamIds;
    }

    public Guid[] PlanningIntervalIds { get; }
    public Guid[]? TeamIds { get; }
}

internal sealed class GetObjectivesForPlanningIntervalsQueryHandler : IQueryHandler<GetObjectivesForPlanningIntervalsQuery, IReadOnlyList<ObjectiveListDto>>
{
    private readonly IGoalsDbContext _goalsDbContext;

    public GetObjectivesForPlanningIntervalsQueryHandler(IGoalsDbContext goalsDbContext)
    {
        _goalsDbContext = goalsDbContext;
    }

    public async Task<IReadOnlyList<ObjectiveListDto>> Handle(GetObjectivesForPlanningIntervalsQuery request, CancellationToken cancellationToken)
    {
        var query = _goalsDbContext.Objectives
            .Where(o => o.PlanId.HasValue && request.PlanningIntervalIds.Contains(o.PlanId.Value))
            .AsQueryable();

        if (request.TeamIds?.Any() ?? false)
        {
            query = query.Where(o => o.OwnerId.HasValue && request.TeamIds.Contains(o.OwnerId.Value));
        }

        return await query
            .ProjectToType<ObjectiveListDto>()
            .ToListAsync(cancellationToken);
    }
}
