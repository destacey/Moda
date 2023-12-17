
using Moda.Planning.Application.PlanningIntervals.Dtos;

namespace Moda.Planning.Application.PlanningIntervals.Queries;
public sealed record GetPlanningIntervalQuery : IQuery<PlanningIntervalDetailsDto?>
{
    public GetPlanningIntervalQuery(Guid planningIntervalId)
    {
        PlanningIntervalId = planningIntervalId;
    }
    public GetPlanningIntervalQuery(int planningIntervalKey)
    {
        PlanningIntervalKey = planningIntervalKey;
    }

    public Guid? PlanningIntervalId { get; }
    public int? PlanningIntervalKey { get; }
}

internal sealed class GetPlanningIntervalQueryHandler : IQueryHandler<GetPlanningIntervalQuery, PlanningIntervalDetailsDto?>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<GetPlanningIntervalQueryHandler> _logger;
    private readonly IDateTimeService _dateTimeService;

    public GetPlanningIntervalQueryHandler(IPlanningDbContext planningDbContext, ILogger<GetPlanningIntervalQueryHandler> logger, IDateTimeService dateTimeService)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
        _dateTimeService = dateTimeService;
    }

    public async Task<PlanningIntervalDetailsDto?> Handle(GetPlanningIntervalQuery request, CancellationToken cancellationToken)
    {
        var query = _planningDbContext.PlanningIntervals
            .Include(p => p.Objectives)
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

        return await query
            .Select(p => PlanningIntervalDetailsDto.Create(p, _dateTimeService))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
