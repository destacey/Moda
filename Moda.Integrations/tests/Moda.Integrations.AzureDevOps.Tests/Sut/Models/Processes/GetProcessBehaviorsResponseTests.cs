using System.Text.Json;
using Moda.Integrations.AzureDevOps.Models.Processes;
using Moda.Integrations.AzureDevOps.Tests.Models;

namespace Moda.Integrations.AzureDevOps.Tests.Sut.Models.Processes;
public class GetProcessBehaviorsResponseTests : CommonResponseTest
{
    [Fact]
    public void JsonSerilizer_Deserialize_Succeeds()
    {
        // Arrange
        var json = GetJson();

        // Act
        var actualResponse = JsonSerializer.Deserialize<GetProcessBehaviorsResponse>(json, _options);

        // Assert
        Assert.NotNull(actualResponse);
        actualResponse.Count.Should().Be(7);
        actualResponse.Value.Count.Should().Be(7);
    }

    private static string GetJson()
    {
        return """
            {
                "count": 7,
                "value": [
                    {
                        "name": "Epics",
                        "referenceName": "Microsoft.VSTS.Agile.EpicBacklogBehavior",
                        "color": "E06C00",
                        "rank": 40,
                        "description": "Epic level backlog and board",
                        "customization": "inherited",
                        "inherits": {
                            "behaviorRefName": "System.PortfolioBacklogBehavior",
                            "url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/behaviors/System.PortfolioBacklogBehavior"
                        },
                        "url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/behaviors/Microsoft.VSTS.Agile.EpicBacklogBehavior"
                    },
                    {
                        "name": "Initiatives",
                        "referenceName": "Custom.1bdcd692-19c0-4b8f-9439-356410b60583",
                        "color": "60af49",
                        "rank": 50,
                        "description": null,
                        "customization": "custom",
                        "inherits": {
                            "behaviorRefName": "System.PortfolioBacklogBehavior",
                            "url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/behaviors/System.PortfolioBacklogBehavior"
                        },
                        "url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/behaviors/Custom.1bdcd692-19c0-4b8f-9439-356410b60583"
                    },
                    {
                        "name": "Stories",
                        "referenceName": "System.RequirementBacklogBehavior",
                        "color": "0098C7",
                        "rank": 20,
                        "description": "Requirement level backlog and board",
                        "customization": "inherited",
                        "inherits": {
                            "behaviorRefName": "System.OrderedBehavior",
                            "url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/behaviors/System.OrderedBehavior"
                        },
                        "url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/behaviors/System.RequirementBacklogBehavior"
                    },
                    {
                        "name": "Features",
                        "referenceName": "Microsoft.VSTS.Agile.FeatureBacklogBehavior",
                        "color": "773B93",
                        "rank": 30,
                        "description": "Feature level backlog and board",
                        "customization": "system",
                        "inherits": {
                            "behaviorRefName": "System.PortfolioBacklogBehavior",
                            "url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/behaviors/System.PortfolioBacklogBehavior"
                        },
                        "url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/behaviors/Microsoft.VSTS.Agile.FeatureBacklogBehavior"
                    },
                    {
                        "name": "Ordered",
                        "referenceName": "System.OrderedBehavior",
                        "color": null,
                        "rank": 0,
                        "description": "Enables work items to be ordered relative to other work items",
                        "customization": "system",
                        "inherits": null,
                        "url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/behaviors/System.OrderedBehavior"
                    },
                    {
                        "name": "Tasks",
                        "referenceName": "System.TaskBacklogBehavior",
                        "color": "A4880A",
                        "rank": 10,
                        "description": "Task level backlog and board",
                        "customization": "system",
                        "inherits": {
                            "behaviorRefName": "System.OrderedBehavior",
                            "url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/behaviors/System.OrderedBehavior"
                        },
                        "url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/behaviors/System.TaskBacklogBehavior"
                    },
                    {
                        "name": "Portfolio",
                        "referenceName": "System.PortfolioBacklogBehavior",
                        "color": null,
                        "rank": 0,
                        "description": "Portfolio level backlog and board",
                        "customization": "system",
                        "inherits": {
                            "behaviorRefName": "System.OrderedBehavior",
                            "url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/behaviors/System.OrderedBehavior"
                        },
                        "url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/behaviors/System.PortfolioBacklogBehavior"
                    }
                ]
            }
            """;
    }
}
