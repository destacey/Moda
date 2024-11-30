using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moda.Common.Domain.Employees;

namespace Moda.Common.Application.Persistence;
public interface IModaDbContext
{
    // this dependency is bigger than needed, but most of the extensions methods are leveraging it. 
    DatabaseFacade Database { get; }
    ChangeTracker ChangeTracker { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    EntityEntry Entry(object entity);

    // Common DbSets
    DbSet<Employee> Employees { get; }
    DbSet<ExternalEmployeeBlacklistItem> ExternalEmployeeBlacklistItems { get; }
}
