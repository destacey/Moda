using System.Text.Json;
using Moda.Integrations.AzureDevOps.Models;

namespace Moda.Integrations.AzureDevOps.Tests.Sut.Models;
public class ProcessWorkItemTypeBehaviorsDtoTests
{
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    [Fact]
    public void JsonSerilizer_Deserialize_Succeeds()
    {
        // Arrange
        var json = GetJson();

        // Act
        var actualResponse = JsonSerializer.Deserialize<ProcessWorkItemTypeBehaviorsDto>(json, _options);

        // Assert
        Assert.NotNull(actualResponse);
        actualResponse.IsDefault.Should().BeTrue();
        actualResponse.Behavior.Id.Should().Be("Custom.1bdcd692-19c0-4b8f-9439-356410b60583");
    }

    private static string GetJson()
    {
        return """
           {
           	"behavior": {
           		"id": "Custom.1bdcd692-19c0-4b8f-9439-356410b60583",
           		"url": "https://dev.azure.com/dstacey/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/behaviors/Custom.1bdcd692-19c0-4b8f-9439-356410b60583"
           	},
           	"isDefault": true,
           	"isLegacyDefault": false,
           	"url": "https://dev.azure.com/dstacey/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/behaviors/Custom.1bdcd692-19c0-4b8f-9439-356410b60583"
           }
           """;
    }
}
