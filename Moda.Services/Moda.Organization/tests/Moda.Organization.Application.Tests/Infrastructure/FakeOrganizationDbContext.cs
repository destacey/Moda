using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moda.Common.Domain.Employees;
using Moda.Organization.Application.Persistence;
using Moda.Organization.Application.Teams.Models;
using Moda.Organization.Domain.Models;
using Moda.Tests.Shared.Infrastructure;

namespace Moda.Organization.Application.Tests.Infrastructure;

/// <summary>
/// A test double for IOrganizationDbContext that provides in-memory collections for all DbSets.
/// This eliminates the need for complex Moq setups in tests and is reusable across all Organization domain tests.
/// </summary>
public class FakeOrganizationDbContext : IOrganizationDbContext, IDisposable
{
    // Organization domain entities
    private readonly List<BaseTeam> _baseTeams = [];
    private readonly List<Team> _teams = [];
    private readonly List<TeamOfTeams> _teamOfTeams = [];
    
    // Common domain entities
    private readonly List<Employee> _employees = [];
    private readonly List<ExternalEmployeeBlacklistItem> _externalEmployeeBlacklistItems = [];

    // DbSet properties
    public DbSet<BaseTeam> BaseTeams => _baseTeams.AsDbSet();
    public DbSet<Team> Teams => _teams.AsDbSet();
    public DbSet<TeamOfTeams> TeamOfTeams => _teamOfTeams.AsDbSet();
    public DbSet<Employee> Employees => _employees.AsDbSet();
    public DbSet<ExternalEmployeeBlacklistItem> ExternalEmployeeBlacklistItems => _externalEmployeeBlacklistItems.AsDbSet();

    // ChangeTracker - we can't create a real one, so we return null and the handler uses defensive coding
    public ChangeTracker ChangeTracker => null!;
    
    public DatabaseFacade Database => throw new NotImplementedException("Database operations are not supported in FakeOrganizationDbContext. Use integration tests with a real DbContext for database-specific functionality.");

    /// <summary>
    /// Gets the number of times SaveChangesAsync has been called.
    /// Useful for asserting that the handler saved changes at the expected times.
    /// </summary>
    public int SaveChangesCallCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        
        // Return the total number of entities as a simple success indicator
        var count = _baseTeams.Count + _teams.Count + _teamOfTeams.Count + _employees.Count + _externalEmployeeBlacklistItems.Count;
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

    #region Graph Table Sync Methods

    /// <summary>
    /// Graph table sync operations are not supported in the fake context.
    /// These require actual database operations with SQL graph tables.
    /// Use integration tests with a real DbContext for testing graph functionality.
    /// </summary>
    public Task<int> UpsertTeamNode(TeamNode teamNode, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Graph table sync operations are not supported in FakeOrganizationDbContext. Use integration tests with a real DbContext for graph-specific functionality.");
    }

    /// <summary>
    /// Graph table sync operations are not supported in the fake context.
    /// These require actual database operations with SQL graph tables.
    /// Use integration tests with a real DbContext for testing graph functionality.
    /// </summary>
    public Task<int> UpsertTeamMembershipEdge(TeamMembershipEdge teamMembershipEdge, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Graph table sync operations are not supported in FakeOrganizationDbContext. Use integration tests with a real DbContext for graph-specific functionality.");
    }

    /// <summary>
    /// Graph table sync operations are not supported in the fake context.
    /// These require actual database operations with SQL graph tables.
    /// Use integration tests with a real DbContext for testing graph functionality.
    /// </summary>
    public Task<int> DeleteTeamMembershipEdge(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Graph table sync operations are not supported in FakeOrganizationDbContext. Use integration tests with a real DbContext for graph-specific functionality.");
    }

    #endregion

    #region Helper Methods for Test Setup

    // BaseTeam
    public void AddBaseTeam(BaseTeam baseTeam) => _baseTeams.Add(baseTeam);
    public void AddBaseTeams(IEnumerable<BaseTeam> baseTeams) => _baseTeams.AddRange(baseTeams);
    
    // Team
    public void AddTeam(Team team)
    {
        _teams.Add(team);
        _baseTeams.Add(team);
    }
    
    public void AddTeams(IEnumerable<Team> teams)
    {
        _teams.AddRange(teams);
        _baseTeams.AddRange(teams);
    }
    
    // TeamOfTeams
    public void AddTeamOfTeams(TeamOfTeams teamOfTeams)
    {
        _teamOfTeams.Add(teamOfTeams);
        _baseTeams.Add(teamOfTeams);
    }
    
    public void AddTeamOfTeams(IEnumerable<TeamOfTeams> teamOfTeams)
    {
        _teamOfTeams.AddRange(teamOfTeams);
        _baseTeams.AddRange(teamOfTeams);
    }
    
    // Employee
    public void AddEmployee(Employee employee) => _employees.Add(employee);
    public void AddEmployees(IEnumerable<Employee> employees) => _employees.AddRange(employees);

    /// <summary>
    /// Clears all entities from the fake context and resets counters.
    /// Useful for test cleanup or test isolation.
    /// </summary>
    public void Clear()
    {
        _baseTeams.Clear();
        _teams.Clear();
        _teamOfTeams.Clear();
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
