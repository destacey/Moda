using Moda.Integrations.AzureDevOps.Extensions;
using Moda.Integrations.AzureDevOps.Models.Projects;

namespace Moda.Integrations.AzureDevOps.Tests.Sut.Extensions;

public class IterationNodeDtoExtensionsTests
{
    [Fact]
    public void SetTeamIds_ShouldAssignTeamId_WhenMappingExists()
    {
        // Arrange
        var root = new IterationNodeDto
        {
            Id = 1,
            Identifier = Guid.NewGuid(),
            Name = "Root",
            Path = "\\Root\\Iteration",
            Children = []
        };
        var teamId = Guid.NewGuid();
        var iterationTeamMappings = new Dictionary<Guid, Guid>
        {
            { root.Identifier, teamId }
        };

        // Act
        var result = root.SetTeamIds(iterationTeamMappings);

        // Assert
        Assert.Equal(teamId, result.TeamId);
    }

    [Fact]
    public void SetTeamIds_WithChildrenShouldAssignTeamId_WhenMappingExists()
    {
        // Arrange
        var child2Id = Guid.NewGuid();
        var root = new IterationNodeDto
        {
            Id = 1,
            Identifier = Guid.NewGuid(),
            Name = "Root",
            Path = "\\Root\\Iteration",
            Children =
                [
                    new IterationNodeDto
                    {
                        Id = 2,
                        Identifier = Guid.NewGuid(),
                        Name = "Child 1",
                        Path = "\\Root\\Iteration\\Child 1",
                    },
                    new IterationNodeDto
                    {
                        Id = 2,
                        Identifier = child2Id,
                        Name = "Child 2",
                        Path = "\\Root\\Iteration\\Child 2",
                    }
                ]
        };
        var rootTeamId = Guid.NewGuid();
        var child2TeamId = Guid.NewGuid();
        var iterationTeamMappings = new Dictionary<Guid, Guid>
        {
            { root.Identifier, rootTeamId },
            { child2Id, child2TeamId }
        };

        // Act
        var result = root.SetTeamIds(iterationTeamMappings);

        // Assert
        Assert.Equal(rootTeamId, result.TeamId);
        Assert.NotNull(result.Children);
        Assert.Equal(rootTeamId, result.Children[0].TeamId);
        Assert.Equal(child2TeamId, result.Children[1].TeamId);
    }

    [Fact]
    public void SetTeamIds_ShouldAssignParentTeamId_WhenMappingDoesNotExist()
    {
        // Arrange
        var root = new IterationNodeDto
        {
            Id = 1,
            Identifier = Guid.NewGuid(),
            Name = "Root",
            Path = "\\Root\\Iteration",
            Children =
                [
                    new IterationNodeDto
                    {
                        Id = 2,
                        Identifier = Guid.NewGuid(),
                        Name = "Child",
                        Path = "\\Root\\Iteration\\Child",
                    }
                ]
        };
        var teamId = Guid.NewGuid();
        var iterationTeamMappings = new Dictionary<Guid, Guid>
        {
            { root.Identifier, teamId }
        };

        // Act
        var result = root.SetTeamIds(iterationTeamMappings);

        // Assert
        Assert.Equal(teamId, result.TeamId);
        Assert.NotNull(result.Children);
        Assert.Equal(teamId, result.Children[0].TeamId);
    }

    [Fact]
    public void SetTeamIds_ShouldHandleEmptyChildren()
    {
        // Arrange
        var root = new IterationNodeDto
        {
            Id = 1,
            Identifier = Guid.NewGuid(),
            Name = "Root",
            Path = "\\Root\\Iteration",
            Children = []
        };
        var teamId = Guid.NewGuid();
        var iterationTeamMappings = new Dictionary<Guid, Guid>
        {
            { root.Identifier, teamId }
        };

        // Act
        var result = root.SetTeamIds(iterationTeamMappings);

        // Assert
        Assert.NotNull(result.Children);
        Assert.Empty(result.Children);
    }

    [Fact]
    public void SetTeamIds_ShouldHandleNullChildren()
    {
        // Arrange
        var root = new IterationNodeDto
        {
            Id = 1,
            Identifier = Guid.NewGuid(),
            Name = "Root",
            Path = "\\Root\\Iteration",
            Children = null
        };
        var iterationTeamMappings = new Dictionary<Guid, Guid>();

        // Act
        var result = root.SetTeamIds(iterationTeamMappings);

        // Assert
        Assert.Null(result.Children);
    }

    [Fact]
    public void SetTeamIds_ShouldHandleNoIterationTeamMapping()
    {
        // Arrange
        var root = new IterationNodeDto
        {
            Id = 1,
            Identifier = Guid.NewGuid(),
            Name = "Root",
            Path = "\\Root\\Iteration",
        };
        var iterationTeamMappings = new Dictionary<Guid, Guid>();

        // Act
        var result = root.SetTeamIds(iterationTeamMappings);

        // Assert
        Assert.Null(result.TeamId);
        Assert.Null(result.Children);
    }
}
