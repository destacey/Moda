namespace Moda.Common.Application.Interfaces;

public interface IModaDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
