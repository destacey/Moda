using Microsoft.EntityFrameworkCore;
using Moq;

namespace Moda.Tests.Shared.Infrastructure;

/// <summary>
/// Provides helper methods for creating mock DbSets for testing.
/// These mocks support async LINQ queries through TestAsyncQueryProvider.
/// </summary>
public static class MockDbSetFactory
{
    /// <summary>
    /// Creates a mock DbSet from a list of data that supports async LINQ queries.
    /// This is useful for unit testing handlers that query EF Core DbSets.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="data">The in-memory data to query</param>
    /// <returns>A mocked DbSet that supports async queries</returns>
    public static DbSet<T> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();

        // Setup async enumeration support
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

        // Setup async query provider
        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));

        // Setup standard IQueryable members
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        return mockSet.Object;
    }
}
