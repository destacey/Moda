using System.Text.Json;
using Moda.Integrations.AzureDevOps.Models;

namespace Moda.Integrations.AzureDevOps.Tests.Sut.Models;
public class ProcessWorkItemStateDtoTests
{
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    [Fact]
    public void JsonSerilizer_Deserialize_Succeeds()
    {
        // Arrange
        var json = GetJson();

        // Act
        var actualResponse = JsonSerializer.Deserialize<ProcessWorkItemStateDto>(json, _options);

        // Assert
        Assert.NotNull(actualResponse);
        actualResponse.Id.Should().Be("28db9425-a6bc-4932-b93d-6a789fb233b3");
        actualResponse.Name.Should().Be("Removed");
        actualResponse.StateCategory.Should().Be("Removed");
    }

    private static string GetJson()
    {
        return """
           {
           	"id": "28db9425-a6bc-4932-b93d-6a789fb233b3",
           	"name": "Removed",
           	"color": "f06673",
           	"stateCategory": "Removed",
           	"order": 4,
           	"url": "https://dev.azure.com/dstacey/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/workItemTypes/ModaAgileProcess.Initiative/states/28db9425-a6bc-4932-b93d-6a789fb233b3",
           	"customizationType": "custom"
           }
           """;
    }
}
