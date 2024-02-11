using MediatR;
using Moda.Common.Application.Dtos;
using Moda.Goals.Application.Objectives.Queries;
using Moda.Planning.Application.PlanningIntervals.Dtos;

namespace Moda.Planning.Application.PlanningIntervals.Queries;

public sealed record GetPlanningIntervalObjectiveQuery : IQuery<PlanningIntervalObjectiveDetailsDto?>
{
    public GetPlanningIntervalObjectiveQuery(Guid id, Guid objectiveId)
    {
        Id = id;
        ObjectiveId = objectiveId;
    }

    public GetPlanningIntervalObjectiveQuery(int key, int objectiveKey)
    {
        Key = key;
        ObjectiveKey = objectiveKey;
    }

    public Guid? Id { get; set; }
    public Guid? ObjectiveId { get; }
    public int? Key { get; set; }
    public int? ObjectiveKey { get; }
}

internal sealed class GetPlanningIntervalObjectiveQueryHandler : IQueryHandler<GetPlanningIntervalObjectiveQuery, PlanningIntervalObjectiveDetailsDto?>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<GetPlanningIntervalObjectiveQueryHandler> _logger;
    private readonly ISender _sender;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetPlanningIntervalObjectiveQueryHandler(IPlanningDbContext planningDbContext, ILogger<GetPlanningIntervalObjectiveQueryHandler> logger, ISender sender, IDateTimeProvider dateTimeProvider)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
        _sender = sender;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<PlanningIntervalObjectiveDetailsDto?> Handle(GetPlanningIntervalObjectiveQuery request, CancellationToken cancellationToken)
    {
        var query = _planningDbContext.PlanningIntervals.AsQueryable();

        if (request.Id.HasValue && request.ObjectiveId.HasValue)
        {
            query = query
                .Include(p => p.Objectives.Where(o => o.Id == request.ObjectiveId.Value))
                    .ThenInclude(o => o.Team)
                .Include(p => p.Objectives.Where(o => o.Id == request.ObjectiveId.Value))
                    .ThenInclude(o => o.HealthCheck)
                .Where(p => p.Id == request.Id.Value);
        }
        else if (request.Key.HasValue && request.ObjectiveKey.HasValue)
        {
            query = query
                .Include(p => p.Objectives.Where(o => o.Key == request.ObjectiveKey.Value))
                    .ThenInclude(o => o.Team)
                .Include(p => p.Objectives.Where(o => o.Key == request.ObjectiveKey.Value))
                    .ThenInclude(o => o.HealthCheck)
                .Where(p => p.Key == request.Key.Value);
        }
        else
        {
            ThrowAndLogException(request, "No planning interval id or local id provided.");
        }

        var planningInterval = await query
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(cancellationToken);
        if (planningInterval is null || planningInterval.Objectives.Count != 1
            || (request.ObjectiveId.HasValue && planningInterval.Objectives.First().Id != request.ObjectiveId)
            || (request.ObjectiveKey.HasValue && planningInterval.Objectives.First().Key != request.ObjectiveKey))
            return null;

        // call the objective query handler
        var objective = await _sender.Send(new GetObjectiveForPlanningIntervalQuery(planningInterval.Objectives.First().ObjectiveId, planningInterval.Id), cancellationToken);
        if (objective is null)
            return null;

        var piNavigation = NavigationDto.Create(planningInterval.Id, planningInterval.Key, planningInterval.Name);

        return PlanningIntervalObjectiveDetailsDto.Create(planningInterval.Objectives.First(), objective, piNavigation, _dateTimeProvider.Now);
    }

    private void ThrowAndLogException(GetPlanningIntervalObjectiveQuery request, string message)
    {
        var requestName = request.GetType().Name;
        var exception = new InternalServerException(message);

        _logger.LogError(exception, "Moda Request: Exception for Request {Name} {@Request}. {Message}", requestName, request, message);
        throw exception;
    }
}
