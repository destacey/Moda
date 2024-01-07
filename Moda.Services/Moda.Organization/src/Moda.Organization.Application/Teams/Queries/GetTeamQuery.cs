using Mapster;

namespace Moda.Organization.Application.Teams.Queries;
public sealed record GetTeamQuery : IQuery<TeamDetailsDto?>
{
    public GetTeamQuery(Guid teamId)
    {
        TeamId = teamId;
    }

    public GetTeamQuery(int teamKey)
    {
        TeamKey = teamKey;
    }

    public Guid? TeamId { get; }
    public int? TeamKey { get; }
}

internal sealed class GetTeamQueryHandler : IQueryHandler<GetTeamQuery, TeamDetailsDto?>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly ILogger<GetTeamQueryHandler> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetTeamQueryHandler(IOrganizationDbContext organizationDbContext, ILogger<GetTeamQueryHandler> logger, IDateTimeProvider dateTimeProvider)
    {
        _organizationDbContext = organizationDbContext;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<TeamDetailsDto?> Handle(GetTeamQuery request, CancellationToken cancellationToken)
    {
        var today = _dateTimeProvider.Now.InUtc().Date;
        var query = _organizationDbContext.Teams
            .Include(t => t.ParentMemberships.Where(m => m.DateRange.Start <= today && (!m.DateRange.End.HasValue || today <= m.DateRange.End)))
            .AsQueryable();

        if (request.TeamId.HasValue)
        {
            query = query.Where(e => e.Id == request.TeamId.Value);
        }
        else if (request.TeamKey.HasValue)
        {
            query = query.Where(e => e.Key == request.TeamKey.Value);
        }
        else
        {
            var requestName = request.GetType().Name;
            var exception = new InternalServerException("No team id or local id provided.");

            _logger.LogError(exception, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            throw exception;
        }

        return await query
            .AsNoTrackingWithIdentityResolution()
            .ProjectToType<TeamDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
