using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moda.Common.Domain.Employees;

namespace Moda.Common.Application.Persistence;
public interface IModaDbContext
{
    ChangeTracker ChangeTracker { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    EntityEntry Entry(object entity);

    // Common DbSets
    DbSet<Employee> Employees { get; }
    DbSet<ExternalEmployeeBlacklistItem> ExternalEmployeeBlacklistItems { get; }
}
