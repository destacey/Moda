using Moda.Organization.Application.Models;

namespace Moda.Organization.Application.TeamsOfTeams.Queries;
public sealed record GetTeamMembershipsQuery : IQuery<IReadOnlyList<TeamMembershipsDto>>
{
    public GetTeamMembershipsQuery(Guid teamId)
    {
        TeamId = teamId;
    }

    public GetTeamMembershipsQuery(int teamKey)
    {
        TeamKey = teamKey;
    }

    public Guid? TeamId { get; }
    public int? TeamKey { get; }
}

internal sealed class GetTeamMembershipsQueryHandler : IQueryHandler<GetTeamMembershipsQuery, IReadOnlyList<TeamMembershipsDto>>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly ILogger<GetTeamMembershipsQueryHandler> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetTeamMembershipsQueryHandler(IOrganizationDbContext organizationDbContext, ILogger<GetTeamMembershipsQueryHandler> logger, IDateTimeProvider dateTimeProvider)
    {
        _organizationDbContext = organizationDbContext;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<IReadOnlyList<TeamMembershipsDto>> Handle(GetTeamMembershipsQuery request, CancellationToken cancellationToken)
    {
        var today = _dateTimeProvider.Now.InUtc().Date;
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

        var team = await query
            .SingleAsync(cancellationToken);

        List<TeamMembershipsDto> memberships = new List<TeamMembershipsDto>();
        if (team.ParentMemberships.Any())
        {
            memberships.AddRange(team.ParentMemberships.Select(m => TeamMembershipsDto.Create(m, _dateTimeProvider)).ToList());
        }
        if (team.ChildMemberships.Any())
        {
            memberships.AddRange(team.ChildMemberships.Select(m => TeamMembershipsDto.Create(m, _dateTimeProvider)).ToList());
        }

        return memberships;
    }
}
