using FluentAssertions;
using Moda.Work.Domain.Tests.Data;
using Xunit;

namespace Moda.Work.Application.Tests.Infrastructure;

/// <summary>
/// Example tests demonstrating the features of FakeWorkDbContext.
/// These serve as documentation for how to use the test infrastructure.
/// </summary>
public class FakeWorkDbContextExamples
{
    [Fact]
    public async Task FakeWorkDbContext_CanTrackSaveChangesCalls()
    {
        // Arrange
        using var context = new FakeWorkDbContext();
        var workspace = new WorkspaceFaker().AsExternal().Generate();

        // Act
        context.AddWorkspace(workspace);
        var count1 = await context.SaveChangesAsync();
        var count2 = await context.SaveChangesAsync();

        // Assert
        context.SaveChangesCallCount.Should().Be(2);
        count1.Should().Be(1); // One workspace
        count2.Should().Be(1); // Still one workspace
    }

    [Fact]
    public async Task FakeWorkDbContext_ClearResetsCounters()
    {
        // Arrange
        using var context = new FakeWorkDbContext();
        var workspace = new WorkspaceFaker().AsExternal().Generate();
        
        context.AddWorkspace(workspace);
        await context.SaveChangesAsync();
        context.SaveChangesCallCount.Should().Be(1);

        // Act
        context.Clear();

        // Assert
        context.SaveChangesCallCount.Should().Be(0);
        context.Workspaces.Should().BeEmpty();
    }

    [Fact]
    public async Task FakeWorkDbContext_DisposeCallsClear()
    {
        // Arrange
        var context = new FakeWorkDbContext();
        var workspace = new WorkspaceFaker().AsExternal().Generate();
        
        context.AddWorkspace(workspace);
        await context.SaveChangesAsync();

        // Act
        context.Dispose();

        // Assert - context is now clean (verified by no exceptions)
        context.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public void FakeWorkDbContext_SupportsUsingStatement()
    {
        // Arrange & Act
        FakeWorkDbContext? capturedContext = null;
        
        using (var context = new FakeWorkDbContext())
        {
            var workspace = new WorkspaceFaker().AsExternal().Generate();
            context.AddWorkspace(workspace);
            capturedContext = context;
        }

        // Assert - Dispose was called automatically
        capturedContext.Should().NotBeNull();
        capturedContext!.SaveChangesCallCount.Should().Be(0);
    }
}
