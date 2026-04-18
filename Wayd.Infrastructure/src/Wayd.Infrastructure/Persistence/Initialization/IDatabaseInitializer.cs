namespace Wayd.Infrastructure.Persistence.Initialization;

internal interface IDatabaseInitializer
{
    Task InitializeDatabase(CancellationToken cancellationToken);
}