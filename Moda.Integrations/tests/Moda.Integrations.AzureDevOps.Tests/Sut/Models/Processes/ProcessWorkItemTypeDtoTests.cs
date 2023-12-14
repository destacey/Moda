using System.Text.Json;
using Moda.Integrations.AzureDevOps.Models.Processes;
using Moda.Integrations.AzureDevOps.Tests.Models;

namespace Moda.Integrations.AzureDevOps.Tests.Sut.Models.Processes;
public class ProcessWorkItemTypeDtoTests : CommonResponseOptions
{
    [Fact]
    public void JsonSerilizer_Deserialize_Succeeds()
    {
        // Arrange
        var json = GetJson();

        // Act
        var actualResponse = JsonSerializer.Deserialize<ProcessWorkItemTypeDto>(json, _options);

        // Assert
        Assert.NotNull(actualResponse);
        actualResponse.ReferenceName.Should().Be("ModaAgileProcess.Epic");
        actualResponse.Name.Should().Be("Epic");
        actualResponse.Description.Should().Be("Epics help teams effectively manage and groom their product backlog");
        actualResponse.IsDisabled.Should().BeFalse();
        actualResponse.Inherits.Should().Be("Microsoft.VSTS.WorkItemTypes.Epic");
        actualResponse.States.Should().HaveCount(6);
        actualResponse.Behaviors.Should().HaveCount(1);
    }

    [Fact]
    public void JsonSerilizer_DeserializeWithoutBehavior_Succeeds()
    {
        // Arrange
        var json = GetJsonWithoutBehavior();

        // Act
        var actualResponse = JsonSerializer.Deserialize<ProcessWorkItemTypeDto>(json, _options);

        // Assert
        Assert.NotNull(actualResponse);
        actualResponse.ReferenceName.Should().Be("Microsoft.VSTS.WorkItemTypes.Bug");
        actualResponse.Name.Should().Be("Bug");
        actualResponse.Description.Should().Be("Describes a divergence between required and actual behavior, and tracks the work done to correct the defect and verify the correction.");
        actualResponse.IsDisabled.Should().BeFalse();
        actualResponse.Inherits.Should().BeNull();
        actualResponse.States.Should().HaveCount(4);
        actualResponse.Behaviors.Should().HaveCount(0);
    }

    private static string GetJson()
    {
        return """
           {
           	"referenceName": "ModaAgileProcess.Epic",
           	"name": "Epic",
           	"description": "Epics help teams effectively manage and groom their product backlog",
           	"url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/workItemTypes/ModaAgileProcess.Epic",
           	"customization": "inherited",
           	"color": "E06C00",
           	"icon": "icon_crown",
           	"isDisabled": false,
           	"inherits": "Microsoft.VSTS.WorkItemTypes.Epic",
           	"states": [
           		{
           			"id": "7b7e3e8c-e500-40b6-ad56-d59b8d64d757",
           			"name": "New",
           			"color": "b2b2b2",
           			"stateCategory": "Proposed",
           			"order": 1,
           			"url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/workItemTypes/ModaAgileProcess.Epic/states/7b7e3e8c-e500-40b6-ad56-d59b8d64d757",
           			"customizationType": "system"
           		},
           		{
           			"id": "277237cd-0bc0-4ffb-bdc6-d358b154ba9e",
           			"name": "Active",
           			"color": "007acc",
           			"stateCategory": "InProgress",
           			"order": 2,
           			"url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/workItemTypes/ModaAgileProcess.Epic/states/277237cd-0bc0-4ffb-bdc6-d358b154ba9e",
           			"customizationType": "system"
           		},
           		{
           			"id": "f36cfea7-889a-448e-b5d1-fbc9b134ec82",
           			"name": "Resolved",
           			"color": "007acc",
           			"stateCategory": "InProgress",
           			"order": 3,
           			"url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/workItemTypes/ModaAgileProcess.Epic/states/f36cfea7-889a-448e-b5d1-fbc9b134ec82",
           			"customizationType": "system"
           		},
           		{
           			"id": "9f479b88-4542-4f9d-8048-5d9c953b5082",
           			"name": "Closed",
           			"color": "339933",
           			"stateCategory": "Completed",
           			"order": 4,
           			"url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/workItemTypes/ModaAgileProcess.Epic/states/9f479b88-4542-4f9d-8048-5d9c953b5082",
           			"customizationType": "system"
           		},
           		{
           			"id": "0293a2ce-2a42-4d0e-bbbf-d2237efa0db8",
           			"name": "Removed",
           			"color": "ffffff",
           			"stateCategory": "Removed",
           			"order": 5,
           			"url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/workItemTypes/ModaAgileProcess.Epic/states/0293a2ce-2a42-4d0e-bbbf-d2237efa0db8",
           			"customizationType": "system"
           		},
           		{
           			"id": "f36cfea7-889a-448e-b5d1-fbc9b134ec82",
           			"name": "Resolved",
           			"color": "007acc",
           			"stateCategory": "InProgress",
           			"order": 6,
           			"url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/workItemTypes/ModaAgileProcess.Epic/states/f36cfea7-889a-448e-b5d1-fbc9b134ec82",
           			"hidden": true,
           			"customizationType": "inherited"
           		}
           	],
           	"behaviors": [
           		{
           			"behavior": {
           				"id": "Microsoft.VSTS.Agile.EpicBacklogBehavior",
           				"url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/behaviors/Microsoft.VSTS.Agile.EpicBacklogBehavior"
           			},
           			"isDefault": true,
           			"isLegacyDefault": true,
           			"url": "https://dev.azure.com/test/_apis/work/processes/15e0d0dd-8d89-46ae-ad4a-7c1d9742bd77/behaviors/Microsoft.VSTS.Agile.EpicBacklogBehavior"
           		}
           	]
           }
           """;
    }

    private static string GetJsonWithoutBehavior()
    {
        return """
            {
                "referenceName": "Microsoft.VSTS.WorkItemTypes.Bug",
                "name": "Bug",
                "description": "Describes a divergence between required and actual behavior, and tracks the work done to correct the defect and verify the correction.",
                "customization": "system",
                "color": "CC293D",
                "icon": "icon_insect",
                "isDisabled": false,
                "inherits": null,
                "states": [
                    {
                        "id": "7b7e3e8c-e500-40b6-ad56-d59b8d64d757",
                        "name": "New",
                        "color": "b2b2b2",
                        "stateCategory": "Proposed",
                        "order": 1,
                        "customizationType": "system"
                    },
                    {
                        "id": "277237cd-0bc0-4ffb-bdc6-d358b154ba9e",
                        "name": "Active",
                        "color": "007acc",
                        "stateCategory": "InProgress",
                        "order": 2,
                        "customizationType": "system"
                    },
                    {
                        "id": "f36cfea7-889a-448e-b5d1-fbc9b134ec82",
                        "name": "Resolved",
                        "color": "ff9d00",
                        "stateCategory": "Resolved",
                        "order": 3,
                        "customizationType": "system"
                    },
                    {
                        "id": "9f479b88-4542-4f9d-8048-5d9c953b5082",
                        "name": "Closed",
                        "color": "339933",
                        "stateCategory": "Completed",
                        "order": 4,
                        "customizationType": "system"
                    }
                ],
                "behaviors": []
            }
            """;
    }
}
