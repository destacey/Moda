using Mapster;
using Microsoft.EntityFrameworkCore;
using Moda.Common.Application.Interfaces;

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
