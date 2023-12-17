﻿using Moda.Organization.Domain.Enums;
using Moda.Planning.Application.Models;
using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.PlanningIntervals.Queries;
public sealed record GetPlanningIntervalPredictabilityQuery(Guid Id) : IQuery<PlanningIntervalPredictabilityDto?>;

internal sealed class GetPlanningIntervalPredictabilityQueryHandler : IQueryHandler<GetPlanningIntervalPredictabilityQuery, PlanningIntervalPredictabilityDto?>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<GetPlanningIntervalPredictabilityQueryHandler> _logger;
    private readonly IDateTimeService _dateTimeService;

    public GetPlanningIntervalPredictabilityQueryHandler(IPlanningDbContext planningDbContext, ILogger<GetPlanningIntervalPredictabilityQueryHandler> logger, IDateTimeService dateTimeService)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
        _dateTimeService = dateTimeService;
    }

    public async Task<PlanningIntervalPredictabilityDto?> Handle(GetPlanningIntervalPredictabilityQuery request, CancellationToken cancellationToken)
    {
        // TODO: filter by teams only.  Don't include teams or objective data for team of teams.
        var planningInterval = await _planningDbContext.PlanningIntervals
            .Include(p => p.Teams.Where(t => t.Team.Type == TeamType.Team))
                .ThenInclude(t => t.Team)
            .Include(p => p.Objectives.Where(o => o.Type == PlanningIntervalObjectiveType.Team))
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (planningInterval is null || !planningInterval.Teams.Any() || !planningInterval.Objectives.Any())
            return null;

        var currentDate = _dateTimeService.Now.InUtc().Date;

        var teamPredictabilities = planningInterval.Teams
            .Select(t => new PlanningIntervalTeamPredictabilityDto(t.Team, planningInterval.CalculatePredictability(currentDate, t.TeamId) ?? 0))
            .OrderBy(t => t.Team.Name)
            .ToList();

        return new PlanningIntervalPredictabilityDto(planningInterval.CalculatePredictability(currentDate) ?? 0, teamPredictabilities);
    }
}

public sealed record PlanningIntervalPredictabilityDto(double Predictability, List<PlanningIntervalTeamPredictabilityDto> TeamPredictabilities);

public sealed record PlanningIntervalTeamPredictabilityDto
{
    public PlanningIntervalTeamPredictabilityDto(PlanningTeam team, double predictability)
    {
        Team = PlanningTeamNavigationDto.FromPlanningTeam(team);
        Predictability = predictability;
    }

    public PlanningTeamNavigationDto Team { get; set; }
    public double Predictability { get; set; }
}