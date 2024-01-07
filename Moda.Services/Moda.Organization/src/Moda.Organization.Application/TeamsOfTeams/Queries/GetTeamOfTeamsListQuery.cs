using Mapster;

namespace Moda.Organization.Application.TeamsOfTeams.Queries;
public sealed record GetTeamOfTeamsListQuery : IQuery<IReadOnlyList<TeamOfTeamsListDto>>
{
    public GetTeamOfTeamsListQuery(bool includeInactive = false, IEnumerable<Guid>? ids = null)
    {
        IncludeInactive = includeInactive;
        TeamIds = ids?.ToList() ?? new List<Guid>();
    }

    public bool IncludeInactive { get; }
    public List<Guid> TeamIds { get; }
}

internal sealed class GetTeamOfTeamsListQueryHandler : IQueryHandler<GetTeamOfTeamsListQuery, IReadOnlyList<TeamOfTeamsListDto>>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly IDateTimeProvider _dateTimeManager;

    public GetTeamOfTeamsListQueryHandler(IOrganizationDbContext organizationDbContext, IDateTimeProvider dateTimeManager)
    {
        _organizationDbContext = organizationDbContext;
        _dateTimeManager = dateTimeManager;
    }

    public async Task<IReadOnlyList<TeamOfTeamsListDto>> Handle(GetTeamOfTeamsListQuery request, CancellationToken cancellationToken)
    {
        var today = _dateTimeManager.Now.InUtc().Date;
        var query = _organizationDbContext.TeamOfTeams
            .Include(t => t.ParentMemberships.Where(m => m.DateRange.Start <= today && (!m.DateRange.End.HasValue || today <= m.DateRange.End)))
            .AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(e => e.IsActive);

        if (request.TeamIds.Any())
            query = query.Where(e => request.TeamIds.Contains(e.Id));

        return await query.AsNoTrackingWithIdentityResolution()
            .ProjectToType<TeamOfTeamsListDto>()
            .ToListAsync(cancellationToken);
    }
}
