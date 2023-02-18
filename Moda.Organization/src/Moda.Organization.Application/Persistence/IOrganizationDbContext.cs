using Microsoft.EntityFrameworkCore;

namespace Moda.Organization.Application.Persistence;
public interface IOrganizationDbContext : IModaDbContext
{
    DbSet<Employee> Employees { get; }
    DbSet<Person> People { get; }
    DbSet<BaseTeam> Teams { get; }
}
