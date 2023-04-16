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

internal sealed class GetTeamMembershipsQueryHandler : IQueryHandler<GetTeamQuery, TeamDetailsDto?>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly ILogger<GetTeamMembershipsQueryHandler> _logger;
    private readonly IDateTimeService _dateTimeService;

    public GetTeamMembershipsQueryHandler(IOrganizationDbContext organizationDbContext, ILogger<GetTeamMembershipsQueryHandler> logger, IDateTimeService dateTimeService)
    {
        _organizationDbContext = organizationDbContext;
        _logger = logger;
        _dateTimeService = dateTimeService;
    }

    public async Task<TeamDetailsDto?> Handle(GetTeamQuery request, CancellationToken cancellationToken)
    {
        var today = _dateTimeService.Now.InUtc().Date;
        var query = _organizationDbContext.Teams
            .Include(t => t.ParentMemberships.Where(m => m.DateRange.Start <= today && (!m.DateRange.End.HasValue || today <= m.DateRange.End)))
            .AsQueryable();

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
            .AsNoTrackingWithIdentityResolution()
            .ProjectToType<TeamDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
