using Moda.Planning.Application.PlanningIntervals.Dtos;

namespace Moda.Planning.Application.PlanningIntervals.Queries;
public sealed record GetPlanningIntervalsQuery() : IQuery<IReadOnlyList<PlanningIntervalListDto>>;

internal sealed class GetPlanningIntervalsQueryHandler : IQueryHandler<GetPlanningIntervalsQuery, IReadOnlyList<PlanningIntervalListDto>>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly IDateTimeProvider _dateTimeManager;

    public GetPlanningIntervalsQueryHandler(IPlanningDbContext planningDbContext, IDateTimeProvider dateTimeManager)
    {
        _planningDbContext = planningDbContext;
        _dateTimeManager = dateTimeManager;
    }

    public async Task<IReadOnlyList<PlanningIntervalListDto>> Handle(GetPlanningIntervalsQuery request, CancellationToken cancellationToken)
    {
        return await _planningDbContext.PlanningIntervals
            .Select(p => PlanningIntervalListDto.Create(p, _dateTimeManager))
            .ToListAsync(cancellationToken);
    }
}
