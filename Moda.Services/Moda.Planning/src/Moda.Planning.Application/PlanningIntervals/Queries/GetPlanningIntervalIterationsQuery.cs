using Moda.Planning.Application.PlanningIntervals.Dtos;

namespace Moda.Planning.Application.PlanningIntervals.Queries;
public sealed record GetPlanningIntervalIterationsQuery : IQuery<IReadOnlyList<PlanningIntervalIterationListDto>>
{
    public GetPlanningIntervalIterationsQuery(Guid planningIntervalId)
    {
        PlanningIntervalId = planningIntervalId;
    }
    public GetPlanningIntervalIterationsQuery(int planningIntervalKey)
    {
        PlanningIntervalKey = planningIntervalKey;
    }

    public Guid? PlanningIntervalId { get; }
    public int? PlanningIntervalKey { get; }
}

internal sealed class GetPlanningIntervalIterationsQueryHandler : IQueryHandler<GetPlanningIntervalIterationsQuery, IReadOnlyList<PlanningIntervalIterationListDto>>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<GetPlanningIntervalIterationsQueryHandler> _logger;

    public GetPlanningIntervalIterationsQueryHandler(IPlanningDbContext planningDbContext, ILogger<GetPlanningIntervalIterationsQueryHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PlanningIntervalIterationListDto>> Handle(GetPlanningIntervalIterationsQuery request, CancellationToken cancellationToken)
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

        var iterations = await query
            .SelectMany(p => p.Iterations)
            .ProjectToType<PlanningIntervalIterationListDto>()
            .ToListAsync(cancellationToken);

        return iterations.OrderBy(i => i.Start).ToList();
    }
}
