using Moda.Organization.Application.Models;

namespace Moda.Organization.Application.Teams.Queries;
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
    private readonly IDateTimeProvider _dateTimeManager;

    public GetTeamMembershipsQueryHandler(IOrganizationDbContext organizationDbContext, ILogger<GetTeamMembershipsQueryHandler> logger, IDateTimeProvider dateTimeManager)
    {
        _organizationDbContext = organizationDbContext;
        _logger = logger;
        _dateTimeManager = dateTimeManager;
    }

    public async Task<IReadOnlyList<TeamMembershipsDto>> Handle(GetTeamMembershipsQuery request, CancellationToken cancellationToken)
    {
        var today = _dateTimeManager.Now.InUtc().Date;
        var query = _organizationDbContext.Teams
            .Include(t => t.ParentMemberships)
                .ThenInclude(m => m.Target)
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

        return team.ParentMemberships.Select(m => TeamMembershipsDto.Create(m, _dateTimeManager)).ToList();
    }
}
