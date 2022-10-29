namespace Moda.Common.Application.Persistence;
public interface IModaDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
