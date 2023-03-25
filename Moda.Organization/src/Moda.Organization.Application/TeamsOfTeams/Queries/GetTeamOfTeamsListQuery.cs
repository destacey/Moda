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

    public GetTeamOfTeamsListQueryHandler(IOrganizationDbContext organizationDbContext)
    {
        _organizationDbContext = organizationDbContext;
    }

    public async Task<IReadOnlyList<TeamOfTeamsListDto>> Handle(GetTeamOfTeamsListQuery request, CancellationToken cancellationToken)
    {
        var query = _organizationDbContext.TeamOfTeams.AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(e => e.IsActive);

        return await query.ProjectToType<TeamOfTeamsListDto>().ToListAsync(cancellationToken);
    }
}
