using Wayd.Organization.Application.Teams.Models;

namespace Wayd.Organization.Application.Persistence;

public interface IOrganizationDbContext : IModaDbContext
{
    DbSet<BaseTeam> BaseTeams { get; }
    DbSet<Team> Teams { get; }
    DbSet<TeamOfTeams> TeamOfTeams { get; }
    DbSet<TeamOperatingModel> TeamOperatingModels { get; }

    // Graph Table Syncs
    Task<int> UpsertTeamNode(TeamNode teamNode, CancellationToken cancellationToken);
    Task<int> UpsertTeamMembershipEdge(TeamMembershipEdge teamMembershipEdge, CancellationToken cancellationToken);
    Task<int> DeleteTeamMembershipEdge(Guid id, CancellationToken cancellationToken);
}
