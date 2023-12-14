using System.Text.Json;
using Moda.Integrations.AzureDevOps.Models.Processes;
using Moda.Integrations.AzureDevOps.Tests.Models;

namespace Moda.Integrations.AzureDevOps.Tests.Sut.Models.Processes;
public class BehaviorDtoTests : CommonResponseOptions
{
    [Fact]
    public void JsonSerilizer_Deserialize_Succeeds()
    {
        // Arrange
        var json = GetJson();

        // Act
        var actualResponse = JsonSerializer.Deserialize<BehaviorDto>(json, _options);

        // Assert
        Assert.NotNull(actualResponse);
        actualResponse.ReferenceName.Should().Be("Microsoft.VSTS.Agile.EpicBacklogBehavior");
        actualResponse.Name.Should().Be("Epics");
        actualResponse.Description.Should().Be("Epic level backlog and board");
        actualResponse.Rank.Should().Be(40);

        actualResponse.Customization.Should().Be("inherited");
        actualResponse.Inherits.Should().NotBeNull();
        actualResponse.Inherits.Should().ContainKey("behaviorRefName");
        actualResponse.Inherits!["behaviorRefName"].ToString().Should().Be("System.PortfolioBacklogBehavior");
    }

    [Fact]
    public void JsonSerilizer_DeserializeWithNullInherits_Succeeds()
    {
        // Arrange
        var json = GetJsonWithNullInherits();

        // Act
        var actualResponse = JsonSerializer.Deserialize<BehaviorDto>(json, _options);

        // Assert
        Assert.NotNull(actualResponse);
        actualResponse.ReferenceName.Should().Be("System.OrderedBehavior");
        actualResponse.Name.Should().Be("Ordered");
        actualResponse.Description.Should().Be("Enables work items to be ordered relative to other work items");
        actualResponse.Rank.Should().Be(0);

        actualResponse.Customization.Should().Be("system");
        actualResponse.Inherits.Should().BeNull();
    }

    private static string GetJson()
    {
        return """
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
            }
            """;
    }

    private static string GetJsonWithNullInherits()
    {
        return """
            {
                "name": "Ordered",
                "referenceName": "System.OrderedBehavior",
                "color": null,
                "rank": 0,
                "description": "Enables work items to be ordered relative to other work items",
                "customization": "system",
                "inherits": null,
                "url": "https://dev.azure.com/dstacey/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/behaviors/System.OrderedBehavior"
            }
            """;
    }
}
