using Moda.Planning.Application.PlanningIntervals.Dtos;

namespace Moda.Planning.Application.PlanningIntervals.Queries;
public sealed record GetPlanningIntervalCalendarQuery : IQuery<PlanningIntervalCalendarDto?>
{
    public GetPlanningIntervalCalendarQuery(Guid planningIntervalId)
    {
        Id = planningIntervalId;
    }
    public GetPlanningIntervalCalendarQuery(int planningIntervalKey)
    {
        Key = planningIntervalKey;
    }

    public Guid? Id { get; }
    public int? Key { get; }
}

internal sealed class GetPlanningIntervalCalendarQueryHandler(IPlanningDbContext planningDbContext, ILogger<GetPlanningIntervalCalendarQueryHandler> logger) : IQueryHandler<GetPlanningIntervalCalendarQuery, PlanningIntervalCalendarDto?>
{
    private const string AppRequestName = nameof(GetPlanningIntervalCalendarQuery);

    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly ILogger<GetPlanningIntervalCalendarQueryHandler> _logger = logger;

    public async Task<PlanningIntervalCalendarDto?> Handle(GetPlanningIntervalCalendarQuery request, CancellationToken cancellationToken)
    {
        var query = _planningDbContext.PlanningIntervals
            .Include(p => p.Iterations)
            .AsQueryable();

        if (request.Id.HasValue)
        {
            query = query.Where(e => e.Id == request.Id.Value);
        }
        else if (request.Key.HasValue)
        {
            query = query.Where(e => e.Key == request.Key.Value);
        }
        else
        {
            var exception = new InternalServerException("No planning interval id or key provided.");

            _logger.LogError(exception, "{AppRequestName}: Exception for request {@Request}", AppRequestName, request);
            throw exception;
        }

        var planningInterval = await query
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return planningInterval is null
            ? null
            : PlanningIntervalCalendarDto.Create(planningInterval.GetCalendar());
    }
}

public sealed record PlanningIntervalCalendarDto
{
    /// <summary>
    /// The Planning Interval Id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The Planning Interval Key.
    /// </summary>
    public int Key { get; set; }

    /// <summary>
    /// The Planning Interval Name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The Planning Interval Start Date.
    /// </summary>
    public required LocalDate Start { get; set; }

    /// <summary>
    /// The Planning Interval End Date.
    /// </summary>
    public required LocalDate End { get; set; }

    /// <summary>
    /// The Planning Interval Iteration Schedules.
    /// </summary>
    public required List<LocalScheduleDto> IterationSchedules { get; set; }

    public static PlanningIntervalCalendarDto Create(PlanningIntervalCalendar calendar)
    {
        return new PlanningIntervalCalendarDto
        {
            Id = calendar.Id,
            Name = calendar.Name,
            Start = calendar.DateRange.Start,
            End = calendar.DateRange.End,
            IterationSchedules = calendar.IterationSchedules.Select(i => LocalScheduleDto.Create(i)).ToList()
        };
    }
}
