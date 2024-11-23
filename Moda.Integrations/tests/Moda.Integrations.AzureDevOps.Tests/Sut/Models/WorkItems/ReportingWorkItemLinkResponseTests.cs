using System.Text.Json;
using Moda.Integrations.AzureDevOps.Models.WorkItems;
using Moda.Integrations.AzureDevOps.Tests.Models;

namespace Moda.Integrations.AzureDevOps.Tests.Sut.Models.WorkItems;
public class ReportingWorkItemLinkResponseTests : CommonResponseOptions
{
    [Fact]
    public void JsonSerilizer_Deserialize_Succeeds()
    {
        // Arrange
        var json = GetJson();

        // Act
        var actualResponse = JsonSerializer.Deserialize<ReportingWorkItemLinkResponse>(json, _options);
        Assert.NotNull(actualResponse);
        actualResponse.Rel.Should().Be("System.LinkTypes.Hierarchy");
        actualResponse.SourceId.Should().Be(28);
        actualResponse.TargetId.Should().Be(58);
        actualResponse.ChangedDate.ToUniversalTime().Should().Be(DateTime.Parse("2024-11-09T14:28:13.133Z").ToUniversalTime());
        actualResponse.ChangedBy?.UniqueName.Should().Be("john.doe@test.com");
        actualResponse.Comment.Should().Be("This is a test comment");
        actualResponse.IsActive.Should().BeTrue();
        actualResponse.ChangedOperation.Should().Be("create");
        actualResponse.SourceProjectId.Should().Be(Guid.Parse("f37284b7-762d-49ea-9b7c-b119849dc57a"));
        actualResponse.TargetProjectId.Should().Be(Guid.Parse("f37284b7-762d-49ea-9b7c-b119849dc57a"));
    }

    private static string GetJson()
    {
        return """
            {
                "rel": "System.LinkTypes.Hierarchy",
                "attributes": {
                    "linkType": "System.LinkTypes.Hierarchy-Forward",
                    "sourceId": 28,
                    "targetId": 58,
                    "isActive": true,
                    "changedDate": "2024-11-09T14:28:13.133Z",
                    "changedBy": {
                        "id": "b99e15b7-8a2d-48be-b29a-3d84e3d3e0c0",
                        "displayName": "John Doe",
                        "uniqueName": "john.doe@test.com",
                        "descriptor": "msa.TESTTESTTEST"
                    },
                    "comment": "This is a test comment",
                    "changedOperation": "create",
                    "sourceProjectId": "f37284b7-762d-49ea-9b7c-b119849dc57a",
                    "targetProjectId": "f37284b7-762d-49ea-9b7c-b119849dc57a"
                }
            }
            """;
    }
}
