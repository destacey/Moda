using Moda.Planning.Domain.Interfaces;

namespace Moda.Planning.Application.PlanningIntervals.Queries;
public sealed record GetPlanningIntervalCalendarQuery : IQuery<PlanningIntervalCalendarDto?>
{
    public GetPlanningIntervalCalendarQuery(Guid planningIntervalId)
    {
        PlanningIntervalId = planningIntervalId;
    }
    public GetPlanningIntervalCalendarQuery(int planningIntervalKey)
    {
        PlanningIntervalKey = planningIntervalKey;
    }

    public Guid? PlanningIntervalId { get; }
    public int? PlanningIntervalKey { get; }
}

internal sealed class GetPlanningIntervalCalendarQueryHandler : IQueryHandler<GetPlanningIntervalCalendarQuery, PlanningIntervalCalendarDto?>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<GetPlanningIntervalCalendarQueryHandler> _logger;

    public GetPlanningIntervalCalendarQueryHandler(IPlanningDbContext planningDbContext, ILogger<GetPlanningIntervalCalendarQueryHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
    }

    public async Task<PlanningIntervalCalendarDto?> Handle(GetPlanningIntervalCalendarQuery request, CancellationToken cancellationToken)
    {
        var query = _planningDbContext.PlanningIntervals
            .Include(p => p.Iterations)
            .AsQueryable();

        if (request.PlanningIntervalId.HasValue)
        {
            query = query.Where(e => e.Id == request.PlanningIntervalId.Value);
        }
        else if (request.PlanningIntervalKey.HasValue)
        {
            query = query.Where(e => e.Key == request.PlanningIntervalKey.Value);
        }
        else
        {
            var requestName = request.GetType().Name;
            var exception = new InternalServerException("No planning interval id or local id provided.");

            _logger.LogError(exception, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
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
    /// Planning Interval Id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Planning Interval Name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Planning Interval start and end dates.
    public required LocalDateRange DateRange { get; set; }

    /// <summary>
    /// List of iteration schedules for the planning interval.
    /// </summary>
    public required List<ILocalSchedule> IterationSchedules { get; set; }

    public static PlanningIntervalCalendarDto Create(PlanningIntervalCalendar calendar)
    {
        return new PlanningIntervalCalendarDto
        {
            Id = calendar.Id,
            Name = calendar.Name,
            DateRange = calendar.DateRange,
            IterationSchedules = calendar.IterationSchedules.ToList()
        };
    }
}
