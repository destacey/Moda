using Mapster;

namespace Moda.Organization.Application.Teams.Queries;
public sealed record GetTeamsQuery : IQuery<IReadOnlyList<TeamListDto>>
{
    public GetTeamsQuery(bool includeInactive = false, IEnumerable<Guid>? ids = null)
    {
        IncludeInactive = includeInactive;
        TeamIds = ids?.ToList() ?? new List<Guid>();
    }

    public bool IncludeInactive { get; }
    public List<Guid> TeamIds { get; }
}

internal sealed class GetTeamsQueryHandler : IQueryHandler<GetTeamsQuery, IReadOnlyList<TeamListDto>>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly IDateTimeService _dateTimeService;

    public GetTeamsQueryHandler(IOrganizationDbContext organizationDbContext, IDateTimeService dateTimeService)
    {
        _organizationDbContext = organizationDbContext;
        _dateTimeService = dateTimeService;
    }

    public async Task<IReadOnlyList<TeamListDto>> Handle(GetTeamsQuery request, CancellationToken cancellationToken)
    {
        var today = _dateTimeService.Now.InUtc().Date;
        var query = _organizationDbContext.Teams
            .Include(t => t.ParentMemberships.Where(m => m.DateRange.Start <= today && (!m.DateRange.End.HasValue || today <= m.DateRange.End)))
            .AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(e => e.IsActive);

        if (request.TeamIds.Any())
            query = query.Where(e => request.TeamIds.Contains(e.Id));

        return await query
            .ProjectToType<TeamListDto>()
            .ToListAsync(cancellationToken);
    }
}
