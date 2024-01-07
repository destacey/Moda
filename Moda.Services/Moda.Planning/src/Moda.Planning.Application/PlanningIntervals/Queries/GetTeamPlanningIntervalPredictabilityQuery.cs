using Moda.Organization.Domain.Enums;
using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.PlanningIntervals.Queries;
public sealed record GetTeamPlanningIntervalPredictabilityQuery(Guid Id, Guid TeamId) : IQuery<double?>;

internal sealed class GetTeamPlanningIntervalPredictabilityQueryHandler : IQueryHandler<GetTeamPlanningIntervalPredictabilityQuery, double?>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<GetTeamPlanningIntervalPredictabilityQueryHandler> _logger;
    private readonly IDateTimeProvider _dateTimeManager;

    public GetTeamPlanningIntervalPredictabilityQueryHandler(IPlanningDbContext planningDbContext, ILogger<GetTeamPlanningIntervalPredictabilityQueryHandler> logger, IDateTimeProvider dateTimeManager)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
        _dateTimeManager = dateTimeManager;
    }

    public async Task<double?> Handle(GetTeamPlanningIntervalPredictabilityQuery request, CancellationToken cancellationToken)
    {
        // TODO: filter by team on teams and objectives
        var planningInterval = await _planningDbContext.PlanningIntervals
            .Include(p => p.Teams.Where(t => t.TeamId == request.TeamId && t.Team.Type == TeamType.Team))
            .Include(p => p.Objectives.Where(o => o.TeamId == request.TeamId && o.Type == PlanningIntervalObjectiveType.Team))
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (planningInterval is null || !planningInterval.Teams.Any())
            return null;

        return planningInterval.CalculatePredictability(_dateTimeManager.Now.InUtc().Date, request.TeamId);
    }
}

