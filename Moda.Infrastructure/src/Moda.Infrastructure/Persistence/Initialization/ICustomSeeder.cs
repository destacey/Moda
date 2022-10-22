namespace Moda.Infrastructure.Persistence.Initialization;

public interface ICustomSeeder
{
    Task Initialize(CancellationToken cancellationToken);
}