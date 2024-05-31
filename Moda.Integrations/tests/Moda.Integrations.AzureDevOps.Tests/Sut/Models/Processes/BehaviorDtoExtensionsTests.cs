using System.Text.Json;
using Moda.Common.Domain.Enums.Work;
using Moda.Integrations.AzureDevOps.Models.Processes;
using Moda.Integrations.AzureDevOps.Tests.Models;

namespace Moda.Integrations.AzureDevOps.Tests.Sut.Models.Processes;
public class BehaviorDtoExtensionsTests : CommonResponseOptions
{
    [Fact]
    public void ToAzdoWorkTypeLevels_Succeeds()
    {
        // Arrange
        var json = GetJson();
        var behaviors = JsonSerializer.Deserialize<List<BehaviorDto>>(json, _options);

        // Act
        var levels = behaviors!.ToIExternalWorkTypeLevels();

        // Assert
        Assert.NotNull(levels);
        levels.Count.Should().Be(5);

        levels[0].Id.Should().Be("Microsoft.VSTS.Agile.EpicBacklogBehavior");
        levels[0].Name.Should().Be("Epics");
        levels[0].Description.Should().Be("Epic level backlog and board");
        levels[0].Order.Should().Be(40);
        levels[0].Tier.Should().Be(WorkTypeTier.Portfolio);

        levels[1].Id.Should().Be("Custom.1bdcd692-19c0-4b8f-9439-356410b60583");
        levels[1].Name.Should().Be("Initiatives");
        levels[1].Description.Should().BeNull();
        levels[1].Order.Should().Be(50);
        levels[1].Tier.Should().Be(WorkTypeTier.Portfolio);

        levels[2].Id.Should().Be("System.RequirementBacklogBehavior");
        levels[2].Name.Should().Be("Stories");
        levels[2].Description.Should().Be("Requirement level backlog and board");
        levels[2].Order.Should().Be(20);
        levels[2].Tier.Should().Be(WorkTypeTier.Requirement);

        levels[3].Id.Should().Be("Microsoft.VSTS.Agile.FeatureBacklogBehavior");
        levels[3].Name.Should().Be("Features");
        levels[3].Description.Should().Be("Feature level backlog and board");
        levels[3].Order.Should().Be(30);
        levels[3].Tier.Should().Be(WorkTypeTier.Portfolio);

        levels[4].Id.Should().Be("System.TaskBacklogBehavior");
        levels[4].Name.Should().Be("Tasks");
        levels[4].Description.Should().Be("Task level backlog and board");
        levels[4].Order.Should().Be(10);
        levels[4].Tier.Should().Be(WorkTypeTier.Task);
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
