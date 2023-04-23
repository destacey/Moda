using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Moda.Common.Application.Persistence;
public interface IModaDbContext
{
    ChangeTracker ChangeTracker { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    EntityEntry Entry(object entity);
}
