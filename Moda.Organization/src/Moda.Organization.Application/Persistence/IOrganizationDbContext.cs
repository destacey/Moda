using Microsoft.EntityFrameworkCore;

namespace Moda.Organization.Application.Persistence;
public interface IOrganizationDbContext : IModaDbContext
{
    DbSet<Person> People { get; }
    DbSet<Employee> Employees { get; }
}
