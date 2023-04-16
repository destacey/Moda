using Microsoft.EntityFrameworkCore;
using Moda.Organization.Application.Models;

namespace Moda.Organization.Application.TeamsOfTeams.Queries;
public sealed record GetTeamMembershipsQuery : IQuery<IReadOnlyList<TeamMembershipsDto>>
{
    public GetTeamMembershipsQuery(Guid teamId)
    {
        TeamId = teamId;
    }

    public GetTeamMembershipsQuery(int teamLocalId)
    {
        TeamLocalId = teamLocalId;
    }

    public Guid? TeamId { get; }
    public int? TeamLocalId { get; }
}

internal sealed class GetTeamMembershipsQueryHandler : IQueryHandler<GetTeamMembershipsQuery, IReadOnlyList<TeamMembershipsDto>>
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

    public async Task<IReadOnlyList<TeamMembershipsDto>> Handle(GetTeamMembershipsQuery request, CancellationToken cancellationToken)
    {
        var today = _dateTimeService.Now.InUtc().Date;
        var query = _organizationDbContext.TeamOfTeams
            .Include(t => t.ParentMemberships)
                .ThenInclude(m => m.Target)
            .Include(t => t.ChildMemberships)
                .ThenInclude(m => m.Source)
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

        var team = await query
            .SingleAsync(cancellationToken);

        List<TeamMembershipsDto> memberships = new List<TeamMembershipsDto>();
        if (team.ParentMemberships.Any())
        {
            memberships.AddRange(team.ParentMemberships.Select(m => TeamMembershipsDto.Create(m, _dateTimeService)).ToList());
        }
        if (team.ChildMemberships.Any())
        {
            memberships.AddRange(team.ChildMemberships.Select(m => TeamMembershipsDto.Create(m, _dateTimeService)).ToList());
        }

        return memberships;
    }
}
