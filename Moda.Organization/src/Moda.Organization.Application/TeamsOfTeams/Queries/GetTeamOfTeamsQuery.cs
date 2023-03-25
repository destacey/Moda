using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Moda.Organization.Application.TeamsOfTeams.Queries;
public sealed record GetTeamOfTeamsQuery : IQuery<TeamOfTeamsDetailsDto?>
{
    public GetTeamOfTeamsQuery(Guid teamId)
    {
        TeamOfTeamId = teamId;
    }

    public GetTeamOfTeamsQuery(int teamLocalId)
    {
        TeamOfTeamsLocalId = teamLocalId;
    }

    public Guid? TeamOfTeamId { get; }
    public int? TeamOfTeamsLocalId { get; }
}

internal sealed class GetTeamOfTeamsQueryHandler : IQueryHandler<GetTeamOfTeamsQuery, TeamOfTeamsDetailsDto?>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly ILogger<GetTeamOfTeamsQueryHandler> _logger;

    public GetTeamOfTeamsQueryHandler(IOrganizationDbContext organizationDbContext, ILogger<GetTeamOfTeamsQueryHandler> logger)
    {
        _organizationDbContext = organizationDbContext;
        _logger = logger;
    }

    public async Task<TeamOfTeamsDetailsDto?> Handle(GetTeamOfTeamsQuery request, CancellationToken cancellationToken)
    {
        var query = _organizationDbContext.TeamOfTeams.AsQueryable();

        if (request.TeamOfTeamId.HasValue)
        {
            query = query.Where(e => e.Id == request.TeamOfTeamId.Value);
        }
        else if (request.TeamOfTeamsLocalId.HasValue)
        {
            query = query.Where(e => e.LocalId == request.TeamOfTeamsLocalId.Value);
        }
        else
        {
            var requestName = request.GetType().Name;
            var exception = new InternalServerException("No id or local id provided.");

            _logger.LogError(exception, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            throw exception;
        }

        return await query
            .ProjectToType<TeamOfTeamsDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
