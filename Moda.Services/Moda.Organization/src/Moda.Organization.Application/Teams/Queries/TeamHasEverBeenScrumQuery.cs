using Moda.Organization.Domain.Enums;

namespace Moda.Organization.Application.Teams.Queries;

/// <summary>
/// Checks if a team has ever used the Scrum methodology.
/// Used by the UI to determine whether to show the Sprints tab.
/// </summary>
public sealed record TeamHasEverBeenScrumQuery(Guid TeamId) : IQuery<bool>;

internal sealed class TeamHasEverBeenScrumQueryHandler(IOrganizationDbContext organizationDbContext)
    : IQueryHandler<TeamHasEverBeenScrumQuery, bool>
{
    private readonly IOrganizationDbContext _organizationDbContext = organizationDbContext;

    public async Task<bool> Handle(TeamHasEverBeenScrumQuery request, CancellationToken cancellationToken)
    {
        // Query through Team aggregate
        return await _organizationDbContext.Teams
            .Where(t => t.Id == request.TeamId)
            .SelectMany(t => t.OperatingModels)
            .AnyAsync(m => m.Methodology == Methodology.Scrum, cancellationToken);
    }
}
