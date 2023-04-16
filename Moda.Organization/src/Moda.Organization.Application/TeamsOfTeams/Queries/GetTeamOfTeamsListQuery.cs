using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Moda.Organization.Application.TeamsOfTeams.Queries;
public sealed record GetTeamOfTeamsListQuery : IQuery<IReadOnlyList<TeamOfTeamsListDto>>
{
    public GetTeamOfTeamsListQuery(bool includeInactive = false)
    {
        IncludeInactive = includeInactive;
    }

    public bool IncludeInactive { get; }
}

internal sealed class GetTeamOfTeamsListQueryHandler : IQueryHandler<GetTeamOfTeamsListQuery, IReadOnlyList<TeamOfTeamsListDto>>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly IDateTimeService _dateTimeService;

    public GetTeamOfTeamsListQueryHandler(IOrganizationDbContext organizationDbContext, IDateTimeService dateTimeService)
    {
        _organizationDbContext = organizationDbContext;
        _dateTimeService = dateTimeService;
    }

    public async Task<IReadOnlyList<TeamOfTeamsListDto>> Handle(GetTeamOfTeamsListQuery request, CancellationToken cancellationToken)
    {
        var today = _dateTimeService.Now.InUtc().Date;
        var query = _organizationDbContext.TeamOfTeams
            .Include(t => t.ParentMemberships.Where(m => m.DateRange.Start <= today && (!m.DateRange.End.HasValue || today <= m.DateRange.End)))
            .AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(e => e.IsActive);

        return await query.AsNoTrackingWithIdentityResolution()
            .ProjectToType<TeamOfTeamsListDto>()
            .ToListAsync(cancellationToken);
    }
}
