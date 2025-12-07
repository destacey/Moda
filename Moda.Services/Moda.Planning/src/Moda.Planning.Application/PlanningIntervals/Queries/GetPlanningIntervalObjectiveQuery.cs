using System.Linq.Expressions;
using MediatR;
using Moda.Common.Application.Dtos;
using Moda.Common.Application.Models;
using Moda.Goals.Application.Objectives.Queries;
using Moda.Planning.Application.PlanningIntervals.Dtos;

namespace Moda.Planning.Application.PlanningIntervals.Queries;

public sealed record GetPlanningIntervalObjectiveQuery : IQuery<PlanningIntervalObjectiveDetailsDto?>
{
    public GetPlanningIntervalObjectiveQuery(IdOrKey idOrKey, IdOrKey objectiveIdOrKey)
    {
        PlanningIntervalIdOrKeyFilter = idOrKey.CreateFilter<PlanningInterval>();
        ObjectiveIdOrKeyFilter = objectiveIdOrKey.CreateFilter<PlanningIntervalObjective>();
        ObjectiveIdOrKey = objectiveIdOrKey;
    }

    public Expression<Func<PlanningInterval, bool>> PlanningIntervalIdOrKeyFilter { get; }
    public Expression<Func<PlanningIntervalObjective, bool>> ObjectiveIdOrKeyFilter { get; }

    public IdOrKey ObjectiveIdOrKey { get; }
}

internal sealed class GetPlanningIntervalObjectiveQueryHandler(IPlanningDbContext planningDbContext, ILogger<GetPlanningIntervalObjectiveQueryHandler> logger, ISender sender, IDateTimeProvider dateTimeProvider) : IQueryHandler<GetPlanningIntervalObjectiveQuery, PlanningIntervalObjectiveDetailsDto?>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly ILogger<GetPlanningIntervalObjectiveQueryHandler> _logger = logger;
    private readonly ISender _sender = sender;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<PlanningIntervalObjectiveDetailsDto?> Handle(GetPlanningIntervalObjectiveQuery request, CancellationToken cancellationToken)
    {
        Guid? objectiveId = request.ObjectiveIdOrKey.AsId;
        if (objectiveId is null)
        {
            objectiveId = await GetObjectiveId(request, cancellationToken);
            if (objectiveId == Guid.Empty)
                return null;
        }

        var planningInterval = await _planningDbContext.PlanningIntervals
        .Include(p => p.Objectives.Where(o => o.Id == objectiveId.Value))
            .ThenInclude(o => o.Team)
        .Include(p => p.Objectives.Where(o => o.Id == objectiveId.Value))
            .ThenInclude(o => o.HealthCheck)
        .Where(request.PlanningIntervalIdOrKeyFilter)
        .AsNoTrackingWithIdentityResolution()
        .FirstOrDefaultAsync(cancellationToken);
        if (planningInterval is null || planningInterval.Objectives.Count != 1
            || planningInterval.Objectives.First().Id != objectiveId.Value)
            return null;

        // call the objective query handler
        var objective = await _sender.Send(new GetObjectiveForPlanningIntervalQuery(planningInterval.Objectives.First().ObjectiveId, planningInterval.Id), cancellationToken);
        if (objective is null)
            return null;

        var piNavigation = NavigationDto.Create(planningInterval.Id, planningInterval.Key, planningInterval.Name);

        return PlanningIntervalObjectiveDetailsDto.Create(planningInterval.Objectives.First(), objective, piNavigation, _dateTimeProvider.Now);
    }

    // TODO: move this to a repository
    private async Task<Guid?> GetObjectiveId(GetPlanningIntervalObjectiveQuery request, CancellationToken cancellationToken)
    {
        return await _planningDbContext.PlanningIntervals
            .Where(request.PlanningIntervalIdOrKeyFilter)
            .SelectMany(p => p.Objectives)
            .Where(request.ObjectiveIdOrKeyFilter)
            .Select(o => o.Id) // returns empty guid if no match
            .FirstOrDefaultAsync(cancellationToken);
    }
}
