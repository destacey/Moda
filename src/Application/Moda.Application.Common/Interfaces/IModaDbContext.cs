namespace Moda.Application.Common.Interfaces;

public interface IModaDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
