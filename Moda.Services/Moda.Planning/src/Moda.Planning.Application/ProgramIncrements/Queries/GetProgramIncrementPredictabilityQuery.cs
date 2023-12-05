using Moda.Organization.Domain.Enums;
using Moda.Planning.Application.Models;
using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.ProgramIncrements.Queries;
public sealed record GetProgramIncrementPredictabilityQuery(Guid Id) : IQuery<ProgramIncrementPredictabilityDto?>;

internal sealed class GetProgramIncrementPredictabilityQueryHandler : IQueryHandler<GetProgramIncrementPredictabilityQuery, ProgramIncrementPredictabilityDto?>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<GetProgramIncrementPredictabilityQueryHandler> _logger;
    private readonly IDateTimeService _dateTimeService;

    public GetProgramIncrementPredictabilityQueryHandler(IPlanningDbContext planningDbContext, ILogger<GetProgramIncrementPredictabilityQueryHandler> logger, IDateTimeService dateTimeService)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
        _dateTimeService = dateTimeService;
    }

    public async Task<ProgramIncrementPredictabilityDto?> Handle(GetProgramIncrementPredictabilityQuery request, CancellationToken cancellationToken)
    {
        // TODO: filter by teams only.  Don't include teams or objective data for team of teams.
        var programIncrement = await _planningDbContext.ProgramIncrements
            .Include(p => p.Teams.Where(t => t.Team.Type == TeamType.Team))
                .ThenInclude(t => t.Team)
            .Include(p => p.Objectives.Where(o => o.Type == ProgramIncrementObjectiveType.Team))
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (programIncrement is null || !programIncrement.Teams.Any() || !programIncrement.Objectives.Any())
            return null;

        var currentDate = _dateTimeService.Now.InUtc().Date;

        var teamPredictabilities = programIncrement.Teams
            .Select(t => new ProgramIncrementTeamPredictabilityDto(t.Team, programIncrement.CalculatePredictability(currentDate, t.TeamId) ?? 0))
            .OrderBy(t => t.Team.Name)
            .ToList();

        return new ProgramIncrementPredictabilityDto(programIncrement.CalculatePredictability(currentDate) ?? 0, teamPredictabilities);
    }
}

public sealed record ProgramIncrementPredictabilityDto(double Predictability, List<ProgramIncrementTeamPredictabilityDto> TeamPredictabilities);

public sealed record ProgramIncrementTeamPredictabilityDto
{
    public ProgramIncrementTeamPredictabilityDto(PlanningTeam team, double predictability)
    {
        Team = PlanningTeamNavigationDto.FromPlanningTeam(team);
        Predictability = predictability;
    }

    public PlanningTeamNavigationDto Team { get; set; }
    public double Predictability { get; set; }
}