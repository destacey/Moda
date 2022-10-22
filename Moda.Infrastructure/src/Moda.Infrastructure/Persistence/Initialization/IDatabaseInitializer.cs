namespace Moda.Infrastructure.Persistence.Initialization;

internal interface IDatabaseInitializer
{
    Task InitializeDatabase(CancellationToken cancellationToken);
}