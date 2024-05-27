using System.Text.Json;
using Moda.Common.Domain.Enums.Work;
using Moda.Integrations.AzureDevOps.Models.Processes;
using Moda.Integrations.AzureDevOps.Tests.Models;

namespace Moda.Integrations.AzureDevOps.Tests.Sut.Models.Processes;
public class BehaviorDtoExtensionsTests : CommonResponseOptions
{
    [Fact]
    public void ToAzdoBacklogLevels_Succeeds()
    {
        // Arrange
        var json = GetJson();
        var behaviors = JsonSerializer.Deserialize<List<BehaviorDto>>(json, _options);

        // Act
        var backlogLevels = behaviors!.ToIExternalBacklogLevels();

        // Assert
        Assert.NotNull(backlogLevels);
        backlogLevels.Count.Should().Be(5);

        backlogLevels[0].Id.Should().Be("Microsoft.VSTS.Agile.EpicBacklogBehavior");
        backlogLevels[0].Name.Should().Be("Epics");
        backlogLevels[0].Description.Should().Be("Epic level backlog and board");
        backlogLevels[0].Rank.Should().Be(40);
        backlogLevels[0].BacklogCategory.Should().Be(BacklogCategory.Portfolio);

        backlogLevels[1].Id.Should().Be("Custom.1bdcd692-19c0-4b8f-9439-356410b60583");
        backlogLevels[1].Name.Should().Be("Initiatives");
        backlogLevels[1].Description.Should().BeNull();
        backlogLevels[1].Rank.Should().Be(50);
        backlogLevels[1].BacklogCategory.Should().Be(BacklogCategory.Portfolio);

        backlogLevels[2].Id.Should().Be("System.RequirementBacklogBehavior");
        backlogLevels[2].Name.Should().Be("Stories");
        backlogLevels[2].Description.Should().Be("Requirement level backlog and board");
        backlogLevels[2].Rank.Should().Be(20);
        backlogLevels[2].BacklogCategory.Should().Be(BacklogCategory.Requirement);

        backlogLevels[3].Id.Should().Be("Microsoft.VSTS.Agile.FeatureBacklogBehavior");
        backlogLevels[3].Name.Should().Be("Features");
        backlogLevels[3].Description.Should().Be("Feature level backlog and board");
        backlogLevels[3].Rank.Should().Be(30);
        backlogLevels[3].BacklogCategory.Should().Be(BacklogCategory.Portfolio);

        backlogLevels[4].Id.Should().Be("System.TaskBacklogBehavior");
        backlogLevels[4].Name.Should().Be("Tasks");
        backlogLevels[4].Description.Should().Be("Task level backlog and board");
        backlogLevels[4].Rank.Should().Be(10);
        backlogLevels[4].BacklogCategory.Should().Be(BacklogCategory.Task);
    }

    private static string GetJson()
    {
        return """
            [
                {
                    "name": "Epics",
                    "referenceName": "Microsoft.VSTS.Agile.EpicBacklogBehavior",
                    "color": "E06C00",
                    "rank": 40,
                    "description": "Epic level backlog and board",
                    "customization": "inherited",
                    "inherits": {
                        "behaviorRefName": "System.PortfolioBacklogBehavior"
                    }
                },
                {
                    "name": "Initiatives",
                    "referenceName": "Custom.1bdcd692-19c0-4b8f-9439-356410b60583",
                    "color": "60af49",
                    "rank": 50,
                    "description": null,
                    "customization": "custom",
                    "inherits": {
                        "behaviorRefName": "System.PortfolioBacklogBehavior"
                    }
                },
                {
                    "name": "Stories",
                    "referenceName": "System.RequirementBacklogBehavior",
                    "color": "0098C7",
                    "rank": 20,
                    "description": "Requirement level backlog and board",
                    "customization": "inherited",
                    "inherits": {
                        "behaviorRefName": "System.OrderedBehavior"
                    }
                },
                {
                    "name": "Features",
                    "referenceName": "Microsoft.VSTS.Agile.FeatureBacklogBehavior",
                    "color": "773B93",
                    "rank": 30,
                    "description": "Feature level backlog and board",
                    "customization": "system",
                    "inherits": {
                        "behaviorRefName": "System.PortfolioBacklogBehavior"
                    }
                },
                {
                    "name": "Ordered",
                    "referenceName": "System.OrderedBehavior",
                    "color": null,
                    "rank": 0,
                    "description": "Enables work items to be ordered relative to other work items",
                    "customization": "system",
                    "inherits": null
                },
                {
                    "name": "Tasks",
                    "referenceName": "System.TaskBacklogBehavior",
                    "color": "A4880A",
                    "rank": 10,
                    "description": "Task level backlog and board",
                    "customization": "system",
                    "inherits": {
                        "behaviorRefName": "System.OrderedBehavior"
                    }
                },
                {
                    "name": "Portfolio",
                    "referenceName": "System.PortfolioBacklogBehavior",
                    "color": null,
                    "rank": 0,
                    "description": "Portfolio level backlog and board",
                    "customization": "system",
                    "inherits": {
                        "behaviorRefName": "System.OrderedBehavior"
                    }
                }
            ]
            """;
    }
}
