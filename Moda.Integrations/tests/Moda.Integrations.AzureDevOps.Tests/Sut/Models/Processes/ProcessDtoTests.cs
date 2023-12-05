using System.Text.Json;
using Moda.Integrations.AzureDevOps.Models.Processes;
using Moda.Integrations.AzureDevOps.Tests.Models;

namespace Moda.Integrations.AzureDevOps.Tests.Sut.Models.Processes;
public class ProcessDtoTests : CommonResponseTest
{
    [Fact]
    public void JsonSerilizer_Deserialize_Succeeds()
    {
        // Arrange
        var json = GetJson();

        // Act
        var actualResponse = JsonSerializer.Deserialize<ProcessDto>(json, _options);

        // Assert
        Assert.NotNull(actualResponse);
        actualResponse.TypeId.Should().Be(Guid.Parse("15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77"));
        actualResponse.Name.Should().Be("Moda Agile Process");
        actualResponse.Description.Should().Be("Includes a fourth level initiative work item type");
        Assert.NotNull(actualResponse.Projects);
        actualResponse.Projects.Should().HaveCount(1);
        actualResponse.Projects.First().Id.Should().Be(Guid.Parse("16301a76-6640-4efd-83cf-6e94474f2528"));
        actualResponse.IsEnabled.Should().BeTrue();
    }

    private static string GetJson()
    {
        return """
            {
                "typeId": "15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77",
                "name": "Moda Agile Process",
                "referenceName": "Inherited.15e0d0dd8d8946aead4a7c1d9742bd77",
                "description": "Includes a fourth level initiative work item type",
                "projects": [
                    {
                        "id": "16301a76-6640-4efd-83cf-6e94474f2528",
                        "name": "Moda",
                        "description": "test project for Moda",
                        "url": "vstfs:///Classification/TeamProject/16301a76-6640-4efd-83cf-6e94474f2528"
                    }
                ],
                "parentProcessTypeId": "adcc42ab-9882-485e-a3ed-7678f01f66bc",
                "isEnabled": true,
                "isDefault": false,
                "customizationType": "inherited"
            }
            """;
    }
}
