using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Moda.Infrastructure.Persistence.Initialization;

internal class DatabaseInitializer : IDatabaseInitializer
{
    private readonly ModaDbContext _context;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(ModaDbContext context, IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task InitializeDatabase(CancellationToken cancellationToken)
    {
        await InitializeDb(cancellationToken);
        await InitializeApplicationDb(cancellationToken);
    }

    public async Task InitializeApplicationDb(CancellationToken cancellationToken)
    {
        // First create a new scope
        using var scope = _serviceProvider.CreateScope();

        // Then run the initialization in the new scope
        await scope.ServiceProvider.GetRequiredService<ApplicationDbInitializer>()
            .Initialize(cancellationToken);
    }

    private async Task InitializeDb(CancellationToken cancellationToken)
    {
        if (_context.Database.GetPendingMigrations().Any())
        {
            _logger.LogInformation("Applying Root Migrations.");
            await _context.Database.MigrateAsync(cancellationToken);
        }
    }
}