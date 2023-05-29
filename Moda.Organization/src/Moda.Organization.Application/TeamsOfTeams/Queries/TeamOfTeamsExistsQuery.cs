namespace Moda.Organization.Application.TeamsOfTeams.Queries;
public sealed record TeamOfTeamsExistsQuery : IQuery<bool>
{
    public TeamOfTeamsExistsQuery(Guid teamId)
    {
        TeamId = teamId;
    }

    public TeamOfTeamsExistsQuery(int teamLocalId)
    {
        TeamLocalId = teamLocalId;
    }

    public Guid? TeamId { get; }
    public int? TeamLocalId { get; }
}

internal sealed class TeamOfTeamsExistsQueryHandler : IQueryHandler<TeamOfTeamsExistsQuery, bool>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly ILogger<TeamOfTeamsExistsQueryHandler> _logger;

    public TeamOfTeamsExistsQueryHandler(IOrganizationDbContext organizationDbContext, ILogger<TeamOfTeamsExistsQueryHandler> logger)
    {
        _organizationDbContext = organizationDbContext;
        _logger = logger;
    }

    public async Task<bool> Handle(TeamOfTeamsExistsQuery request, CancellationToken cancellationToken)
    {
        var query = _organizationDbContext.TeamOfTeams.AsQueryable();

        if (request.TeamId.HasValue)
        {
            return await query.AnyAsync(e => e.Id == request.TeamId.Value, cancellationToken);
        }
        else if (request.TeamLocalId.HasValue)
        {
            return await query.AnyAsync(e => e.LocalId == request.TeamLocalId.Value, cancellationToken);
        }
        else
        {
            var requestName = request.GetType().Name;
            var exception = new InternalServerException("No team id or local id provided.");

            _logger.LogError(exception, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            throw exception;
        }
    }
}
