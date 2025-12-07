using Moda.Common.Application.Requests.Goals.Dtos;
using Moda.Common.Application.Requests.Goals.Queries;
using Moda.Goals.Application.Persistence;

namespace Moda.Goals.Application.Objectives.Queries;

internal sealed class GetObjectivesForPlanningIntervalsQueryHandler(IGoalsDbContext goalsDbContext) : IQueryHandler<GetObjectivesForPlanningIntervalsQuery, IReadOnlyList<ObjectiveListDto>>
{
    private readonly IGoalsDbContext _goalsDbContext = goalsDbContext;

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
