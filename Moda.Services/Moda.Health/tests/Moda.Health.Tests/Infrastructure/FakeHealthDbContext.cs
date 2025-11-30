using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moda.Common.Domain.Employees;
using Moda.Common.Domain.Identity;
using Moda.Health;
using Moda.Health.Models;
using Moda.Tests.Shared.Infrastructure;

namespace Moda.Health.Tests.Infrastructure;

/// <summary>
/// A test double for IHealthDbContext that provides in-memory collections for all DbSets.
/// This eliminates the need for complex Moq setups in tests and is reusable across all Health domain tests.
/// </summary>
public class FakeHealthDbContext : IHealthDbContext, IDisposable
{
    // Health domain entities
    private readonly List<HealthCheck> _healthChecks = [];
    
    // Common domain entities
    private readonly List<Employee> _employees = [];
    private readonly List<ExternalEmployeeBlacklistItem> _externalEmployeeBlacklistItems = [];
    private readonly List<PersonalAccessToken> _personalAccessTokens = [];

    // DbSet properties
    public DbSet<HealthCheck> HealthChecks => _healthChecks.AsDbSet();
    public DbSet<Employee> Employees => _employees.AsDbSet();
    public DbSet<ExternalEmployeeBlacklistItem> ExternalEmployeeBlacklistItems => _externalEmployeeBlacklistItems.AsDbSet();
    public DbSet<PersonalAccessToken> PersonalAccessTokens => _personalAccessTokens.AsDbSet();

    // ChangeTracker - we can't create a real one, so we return null and the handler uses defensive coding
    public ChangeTracker ChangeTracker => null!;
    
    public DatabaseFacade Database => throw new NotImplementedException("Database operations are not supported in FakeHealthDbContext. Use integration tests with a real DbContext for database-specific functionality.");

    /// <summary>
    /// Gets the number of times SaveChangesAsync has been called.
    /// Useful for asserting that the handler saved changes at the expected times.
    /// </summary>
    public int SaveChangesCallCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;

        // Return the total number of entities as a simple success indicator
        var count = _healthChecks.Count + _employees.Count + _externalEmployeeBlacklistItems.Count + _personalAccessTokens.Count;
        return Task.FromResult(count);
    }

    public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
    {
        throw new NotImplementedException("Entry tracking is not needed for unit tests. If you need this, consider using integration tests with a real DbContext.");
    }

    public EntityEntry Entry(object entity)
    {
        throw new NotImplementedException("Entry tracking is not needed for unit tests. If you need this, consider using integration tests with a real DbContext.");
    }

    #region Helper Methods for Test Setup

    // HealthCheck
    public void AddHealthCheck(HealthCheck healthCheck) => _healthChecks.Add(healthCheck);
    public void AddHealthChecks(IEnumerable<HealthCheck> healthChecks) => _healthChecks.AddRange(healthChecks);
    
    // Employee
    public void AddEmployee(Employee employee) => _employees.Add(employee);
    public void AddEmployees(IEnumerable<Employee> employees) => _employees.AddRange(employees);

    /// <summary>
    /// Clears all entities from the fake context and resets counters.
    /// Useful for test cleanup or test isolation.
    /// </summary>
    public void Clear()
    {
        _healthChecks.Clear();
        _employees.Clear();
        _externalEmployeeBlacklistItems.Clear();
        _personalAccessTokens.Clear();
        SaveChangesCallCount = 0;
    }

    /// <summary>
    /// Disposes the fake context by clearing all data.
    /// </summary>
    public void Dispose()
    {
        Clear();
        GC.SuppressFinalize(this);
    }

    #endregion
}
