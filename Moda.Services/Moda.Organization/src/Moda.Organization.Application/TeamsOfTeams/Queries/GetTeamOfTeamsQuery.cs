using Mapster;

namespace Moda.Organization.Application.TeamsOfTeams.Queries;
public sealed record GetTeamOfTeamsQuery : IQuery<TeamOfTeamsDetailsDto?>
{
    public GetTeamOfTeamsQuery(Guid teamId)
    {
        TeamOfTeamsId = teamId;
    }

    public GetTeamOfTeamsQuery(int teamKey)
    {
        TeamOfTeamsKey = teamKey;
    }

    public Guid? TeamOfTeamsId { get; }
    public int? TeamOfTeamsKey { get; }
}

internal sealed class GetTeamOfTeamsQueryHandler : IQueryHandler<GetTeamOfTeamsQuery, TeamOfTeamsDetailsDto?>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly ILogger<GetTeamOfTeamsQueryHandler> _logger;
    private readonly IDateTimeService _dateTimeService;

    public GetTeamOfTeamsQueryHandler(IOrganizationDbContext organizationDbContext, ILogger<GetTeamOfTeamsQueryHandler> logger, IDateTimeService dateTimeService)
    {
        _organizationDbContext = organizationDbContext;
        _logger = logger;
        _dateTimeService = dateTimeService;
    }

    public async Task<TeamOfTeamsDetailsDto?> Handle(GetTeamOfTeamsQuery request, CancellationToken cancellationToken)
    {
        var today = _dateTimeService.Now.InUtc().Date;
        var query = _organizationDbContext.TeamOfTeams
            .Include(t => t.ParentMemberships.Where(m => m.DateRange.Start <= today && (!m.DateRange.End.HasValue || today <= m.DateRange.End)))
            .AsQueryable();

        if (request.TeamOfTeamsId.HasValue)
        {
            query = query.Where(e => e.Id == request.TeamOfTeamsId.Value);
        }
        else if (request.TeamOfTeamsKey.HasValue)
        {
            query = query.Where(e => e.Key == request.TeamOfTeamsKey.Value);
        }
        else
        {
            var requestName = request.GetType().Name;
            var exception = new InternalServerException("No id or local id provided.");

            _logger.LogError(exception, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            throw exception;
        }

        return await query
            .AsNoTrackingWithIdentityResolution()
            .ProjectToType<TeamOfTeamsDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
