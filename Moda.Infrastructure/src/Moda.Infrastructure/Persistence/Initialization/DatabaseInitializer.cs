﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Moda.Infrastructure.Persistence.Initialization;

internal class DatabaseInitializer : IDatabaseInitializer
{
    private readonly DatabaseSettings _dbSettings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IOptions<DatabaseSettings> dbSettings, IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
    {
        _dbSettings = dbSettings.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task InitializeDatabasesAsync(CancellationToken cancellationToken)
    {
        // TODO
        return Task.CompletedTask;
    }

    //public async Task InitializeApplicationDbForTenantAsync(FSHTenantInfo tenant, CancellationToken cancellationToken)
    //{
    //    // First create a new scope
    //    using var scope = _serviceProvider.CreateScope();

    //    // Then set current tenant so the right connectionstring is used
    //    _serviceProvider.GetRequiredService<IMultiTenantContextAccessor>()
    //        .MultiTenantContext = new MultiTenantContext<FSHTenantInfo>()
    //        {
    //            TenantInfo = tenant
    //        };

    //    // Then run the initialization in the new scope
    //    await scope.ServiceProvider.GetRequiredService<ApplicationDbInitializer>()
    //        .InitializeAsync(cancellationToken);
    //}

    //private async Task InitializeTenantDbAsync(CancellationToken cancellationToken)
    //{
    //    if (_tenantDbContext.Database.GetPendingMigrations().Any())
    //    {
    //        _logger.LogInformation("Applying Root Migrations.");
    //        await _tenantDbContext.Database.MigrateAsync(cancellationToken);
    //    }

    //    await SeedRootTenantAsync(cancellationToken);
    //}

    //private async Task SeedRootTenantAsync(CancellationToken cancellationToken)
    //{
    //    if (await _tenantDbContext.TenantInfo.FindAsync(new object?[] { MultitenancyConstants.Root.Id }, cancellationToken: cancellationToken) is null)
    //    {
    //        var rootTenant = new FSHTenantInfo(
    //            MultitenancyConstants.Root.Id,
    //            MultitenancyConstants.Root.Name,
    //            _dbSettings.ConnectionString,
    //            MultitenancyConstants.Root.EmailAddress);

    //        rootTenant.SetValidity(DateTime.UtcNow.AddYears(1));

    //        _tenantDbContext.TenantInfo.Add(rootTenant);

    //        await _tenantDbContext.SaveChangesAsync(cancellationToken);
    //    }
    //}
}