using Mapster;

namespace Moda.Organization.Application.Teams.Queries;

/// <summary>
/// Gets all operating models for a team, ordered by start date descending (most recent first).
/// </summary>
public sealed record GetTeamOperatingModelHistoryQuery(Guid TeamId) : IQuery<IReadOnlyList<TeamOperatingModelDto>>;

internal sealed class GetTeamOperatingModelHistoryQueryHandler(IOrganizationDbContext organizationDbContext)
    : IQueryHandler<GetTeamOperatingModelHistoryQuery, IReadOnlyList<TeamOperatingModelDto>>
{
    private readonly IOrganizationDbContext _organizationDbContext = organizationDbContext;

    public async Task<IReadOnlyList<TeamOperatingModelDto>> Handle(GetTeamOperatingModelHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _organizationDbContext.TeamOperatingModels
            .Where(m => m.TeamId == request.TeamId)
            .OrderByDescending(m => m.DateRange.Start)
            .ProjectToType<TeamOperatingModelDto>()
            .ToListAsync(cancellationToken);
    }
}
