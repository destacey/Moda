using Moda.Common.Domain.Enums.Organization;
using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.PlanningIntervals.Queries;
public sealed record GetTeamPlanningIntervalPredictabilityQuery(Guid Id, Guid TeamId) : IQuery<double?>;

internal sealed class GetTeamPlanningIntervalPredictabilityQueryHandler : IQueryHandler<GetTeamPlanningIntervalPredictabilityQuery, double?>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<GetTeamPlanningIntervalPredictabilityQueryHandler> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetTeamPlanningIntervalPredictabilityQueryHandler(IPlanningDbContext planningDbContext, ILogger<GetTeamPlanningIntervalPredictabilityQueryHandler> logger, IDateTimeProvider dateTimeProvider)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
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

        return planningInterval.CalculatePredictability(_dateTimeProvider.Now.InUtc().Date, request.TeamId);
    }
}

