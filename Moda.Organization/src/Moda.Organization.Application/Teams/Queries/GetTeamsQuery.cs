using Mapster;

namespace Moda.Organization.Application.Teams.Queries;
public sealed record GetTeamsQuery : IQuery<IReadOnlyList<TeamListDto>>
{
    public GetTeamsQuery(bool includeInactive = false)
    {
        IncludeInactive = includeInactive;
    }

    public bool IncludeInactive { get; }
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

        return await query.AsNoTrackingWithIdentityResolution()
            .AsNoTrackingWithIdentityResolution()
            .ProjectToType<TeamListDto>()
            .ToListAsync(cancellationToken);
    }
}
