using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moda.AppIntegration.Application.Persistence;
using Moda.AppIntegration.Domain.Models;
using Moda.Common.Domain.Employees;
using Moda.Tests.Shared.Infrastructure;

namespace Moda.AppIntegration.Application.Tests.Infrastructure;

/// <summary>
/// A test double for IAppIntegrationDbContext that provides in-memory collections for all DbSets.
/// This eliminates the need for complex Moq setups in tests and is reusable across all AppIntegration domain tests.
/// </summary>
public class FakeAppIntegrationDbContext : IAppIntegrationDbContext, IDisposable
{
    // AppIntegration domain entities
    private readonly List<Connection> _connections = [];
    private readonly List<AzureDevOpsBoardsConnection> _azureDevOpsBoardsConnections = [];
    
    // Common domain entities
    private readonly List<Employee> _employees = [];
    private readonly List<ExternalEmployeeBlacklistItem> _externalEmployeeBlacklistItems = [];

    // DbSet properties
    public DbSet<Connection> Connections => _connections.AsDbSet();
    public DbSet<AzureDevOpsBoardsConnection> AzureDevOpsBoardsConnections => _azureDevOpsBoardsConnections.AsDbSet();
    public DbSet<Employee> Employees => _employees.AsDbSet();
    public DbSet<ExternalEmployeeBlacklistItem> ExternalEmployeeBlacklistItems => _externalEmployeeBlacklistItems.AsDbSet();

    // ChangeTracker - we can't create a real one, so we return null and the handler uses defensive coding
    public ChangeTracker ChangeTracker => null!;
    
    public DatabaseFacade Database => throw new NotImplementedException("Database operations are not supported in FakeAppIntegrationDbContext. Use integration tests with a real DbContext for database-specific functionality.");

    /// <summary>
    /// Gets the number of times SaveChangesAsync has been called.
    /// Useful for asserting that the handler saved changes at the expected times.
    /// </summary>
    public int SaveChangesCallCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        
        // Return the total number of entities as a simple success indicator
        var count = _connections.Count + _azureDevOpsBoardsConnections.Count + _employees.Count + _externalEmployeeBlacklistItems.Count;
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

    // Connection
    public void AddConnection(Connection connection) => _connections.Add(connection);
    public void AddConnections(IEnumerable<Connection> connections) => _connections.AddRange(connections);
    
    // AzureDevOpsBoardsConnection
    public void AddAzureDevOpsBoardsConnection(AzureDevOpsBoardsConnection connection) => _azureDevOpsBoardsConnections.Add(connection);
    public void AddAzureDevOpsBoardsConnections(IEnumerable<AzureDevOpsBoardsConnection> connections) => _azureDevOpsBoardsConnections.AddRange(connections);
    
    // Employee
    public void AddEmployee(Employee employee) => _employees.Add(employee);
    public void AddEmployees(IEnumerable<Employee> employees) => _employees.AddRange(employees);

    /// <summary>
    /// Clears all entities from the fake context and resets counters.
    /// Useful for test cleanup or test isolation.
    /// </summary>
    public void Clear()
    {
        _connections.Clear();
        _azureDevOpsBoardsConnections.Clear();
        _employees.Clear();
        _externalEmployeeBlacklistItems.Clear();
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
