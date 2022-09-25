namespace Moda.Infrastructure.Persistence.Initialization;

internal interface IDatabaseInitializer
{
    Task InitializeDatabasesAsync(CancellationToken cancellationToken);
}