using Moda.Planning.Application.PlanningIntervals.Dtos;

namespace Moda.Planning.Application.PlanningIntervals.Queries;
public sealed record GetPlanningIntervalsQuery() : IQuery<IReadOnlyList<PlanningIntervalListDto>>;

internal sealed class GetPlanningIntervalsQueryHandler : IQueryHandler<GetPlanningIntervalsQuery, IReadOnlyList<PlanningIntervalListDto>>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly IDateTimeService _dateTimeService;

    public GetPlanningIntervalsQueryHandler(IPlanningDbContext planningDbContext, IDateTimeService dateTimeService)
    {
        _planningDbContext = planningDbContext;
        _dateTimeService = dateTimeService;
    }

    public async Task<IReadOnlyList<PlanningIntervalListDto>> Handle(GetPlanningIntervalsQuery request, CancellationToken cancellationToken)
    {
        return await _planningDbContext.PlanningIntervals
            .Select(p => PlanningIntervalListDto.Create(p, _dateTimeService))
            .ToListAsync(cancellationToken);
    }
}
