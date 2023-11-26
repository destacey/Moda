using System.Text.Json;
using Moda.Integrations.AzureDevOps.Models.Processes;
using Moda.Integrations.AzureDevOps.Tests.Models;

namespace Moda.Integrations.AzureDevOps.Tests.Sut.Models.Processes;
public class BehaviorDtoTests : CommonResponseTest
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
}
