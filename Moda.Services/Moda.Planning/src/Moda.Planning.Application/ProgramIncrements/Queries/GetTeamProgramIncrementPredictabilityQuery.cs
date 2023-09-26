namespace Moda.Planning.Application.ProgramIncrements.Queries;
public sealed record GetTeamProgramIncrementPredictabilityQuery(Guid Id, Guid TeamId) : IQuery<double?>;

internal sealed class GetTeamProgramIncrementPredictabilityQueryHandler : IQueryHandler<GetTeamProgramIncrementPredictabilityQuery, double?>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<GetTeamProgramIncrementPredictabilityQueryHandler> _logger;
    private readonly IDateTimeService _dateTimeService;

    public GetTeamProgramIncrementPredictabilityQueryHandler(IPlanningDbContext planningDbContext, ILogger<GetTeamProgramIncrementPredictabilityQueryHandler> logger, IDateTimeService dateTimeService)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
        _dateTimeService = dateTimeService;
    }

    public async Task<double?> Handle(GetTeamProgramIncrementPredictabilityQuery request, CancellationToken cancellationToken)
    {
        // TODO: filter by team on teams and objectives
        var programIncrement = await _planningDbContext.ProgramIncrements
            .Include(p => p.Teams.Where(t => t.TeamId == request.TeamId))
            .Include(p => p.Objectives.Where(o => o.TeamId == request.TeamId))
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (programIncrement is null || !programIncrement.Teams.Any())
            return null;

        return programIncrement.CalculatePredictability(_dateTimeService.Now.InUtc().Date, request.TeamId);
    }
}

