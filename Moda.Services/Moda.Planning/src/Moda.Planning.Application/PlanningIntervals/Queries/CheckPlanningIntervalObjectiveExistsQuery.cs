namespace Moda.Planning.Application.PlanningIntervals.Queries;
public sealed record CheckPlanningIntervalObjectiveExistsQuery(Guid PlanningIntervalId, Guid ObjectiveId) : IQuery<bool>;

internal sealed class CheckPlanningIntervalObjectiveExistsQueryHandler(IPlanningDbContext planningDbContext) : IQueryHandler<CheckPlanningIntervalObjectiveExistsQuery, bool>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;

    public async Task<bool> Handle(CheckPlanningIntervalObjectiveExistsQuery request, CancellationToken cancellationToken)
    {
        return await _planningDbContext.PlanningIntervals
            .AnyAsync(p => p.Id == request.PlanningIntervalId && p.Objectives.Any(o => o.Id == request.ObjectiveId), cancellationToken);
    }
}
