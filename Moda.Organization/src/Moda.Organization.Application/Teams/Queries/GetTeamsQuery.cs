using Mapster;
using Microsoft.EntityFrameworkCore;

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

    public GetTeamsQueryHandler(IOrganizationDbContext organizationDbContext)
    {
        _organizationDbContext = organizationDbContext;
    }

    public async Task<IReadOnlyList<TeamListDto>> Handle(GetTeamsQuery request, CancellationToken cancellationToken)
    {
        var query = _organizationDbContext.Teams.AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(e => e.IsActive);

        return await query.ProjectToType<TeamListDto>().ToListAsync(cancellationToken);
    }
}
