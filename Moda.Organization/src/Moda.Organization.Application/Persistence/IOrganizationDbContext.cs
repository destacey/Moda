using Microsoft.EntityFrameworkCore;

namespace Moda.Organization.Application.Persistence;
public interface IOrganizationDbContext : IModaDbContext
{
    DbSet<Employee> Employees { get; }
    DbSet<BaseTeam> BaseTeams { get; }
    DbSet<Team> Teams { get; }
    DbSet<TeamOfTeams> TeamOfTeams { get; }
}
