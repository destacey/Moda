using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Moda.Organization.Application.Teams.Queries;
public sealed record GetTeamQuery : IQuery<TeamDetailsDto?>
{
    public GetTeamQuery(Guid teamId)
    {
        TeamId = teamId;
    }

    public GetTeamQuery(int teamLocalId)
    {
        TeamLocalId = teamLocalId;
    }

    public Guid? TeamId { get; }
    public int? TeamLocalId { get; }
}

internal sealed class GetTeamQueryHandler : IQueryHandler<GetTeamQuery, TeamDetailsDto?>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly ILogger<GetTeamQueryHandler> _logger;

    public GetTeamQueryHandler(IOrganizationDbContext organizationDbContext, ILogger<GetTeamQueryHandler> logger)
    {
        _organizationDbContext = organizationDbContext;
        _logger = logger;
    }

    public async Task<TeamDetailsDto?> Handle(GetTeamQuery request, CancellationToken cancellationToken)
    {
        var query = _organizationDbContext.Teams.AsQueryable();

        if (request.TeamId.HasValue)
        {
            query = query.Where(e => e.Id == request.TeamId.Value);
        }
        else if (request.TeamLocalId.HasValue)
        {
            query = query.Where(e => e.LocalId == request.TeamLocalId.Value);
        }
        else
        {
            var requestName = request.GetType().Name;
            var exception = new InternalServerException("No team id or local id provided.");

            _logger.LogError(exception, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            throw exception;
        }

        return await query
            .ProjectToType<TeamDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
