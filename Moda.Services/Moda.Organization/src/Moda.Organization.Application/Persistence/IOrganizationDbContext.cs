namespace Moda.Organization.Application.Persistence;
public interface IOrganizationDbContext : IModaDbContext
{
    DbSet<BaseTeam> BaseTeams { get; }
    DbSet<Team> Teams { get; }
    DbSet<TeamOfTeams> TeamOfTeams { get; }
}
