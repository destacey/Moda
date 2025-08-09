using System.Text.Json;
using Moda.Common.Extensions;
using Moda.Integrations.AzureDevOps.Models;
using Moda.Integrations.AzureDevOps.Tests.Models;

namespace Moda.Integrations.AzureDevOps.Tests.Sut.Models;
public class IterationNodeResponseTests : CommonResponseOptions
{
    [Fact]
    public void JsonSerilizer_Deserialize_Succeeds()
    {
        // Arrange
        var json = GetJson();

        // Act
        var actualResponse = JsonSerializer.Deserialize<IterationNodeResponse>(json, _options);

        // Assert
        Assert.NotNull(actualResponse);
        actualResponse.Id.Should().Be(127);
        actualResponse.Identifier.Should().Be(Guid.Parse("68429463-523f-4c69-8c16-4321543db2e4"));
        actualResponse.Name.Should().Be("Moda");
        actualResponse.Children.Should().NotBeNull();
        actualResponse.Children!.Count.Should().Be(2);
        actualResponse.Path.Should().Be("\\Moda\\Iteration");

        var list = actualResponse.FlattenHierarchy(a => a.Children).ToList();

        list.Count.Should().Be(14);

        var iterationNoDates = list.First(i => i.Id == 421);
        iterationNoDates.Attributes?.StartDate.Should().BeNull();
        iterationNoDates.Attributes?.EndDate.Should().BeNull();

        var iterationWithDates = list.First(i => i.Id == 426);
        iterationWithDates.Attributes?.StartDate.Should().Be(new DateTime(2022, 9, 26, 0, 0, 0, DateTimeKind.Utc));
        iterationWithDates.Attributes?.EndDate.Should().Be(new DateTime(2022, 10, 9, 0, 0, 0, DateTimeKind.Utc));
    }

    private static string GetJson()
    {
        return """
            {
                "id": 127,
                "identifier": "68429463-523f-4c69-8c16-4321543db2e4",
                "name": "Moda",
                "structureType": "iteration",
                "hasChildren": true,
                "children": [
                    {
                        "id": 421,
                        "identifier": "8f218f2f-dc77-4f6a-af01-6d39deb89cca",
                        "name": "Moda Integrations Team",
                        "structureType": "iteration",
                        "hasChildren": true,
                        "children": [
                            {
                                "id": 32,
                                "identifier": "56503f9a-075b-4688-b83b-135c2adc982d",
                                "name": "Test",
                                "structureType": "iteration",
                                "hasChildren": false,
                                "path": "\\Moda\\Iteration\\Moda Integrations Team\\Test",
                                "url": "https://dev.azure.com/dstacey/3b15d01e-d259-48eb-a15c-dd29384fd598/_apis/wit/classificationNodes/Iterations/Moda%20Integrations%20Team/Test"
                            }
                        ],
                        "path": "\\Moda\\Iteration\\Moda Integrations Team",
                        "url": "https://dev.azure.com/dstacey/3b15d01e-d259-48eb-a15c-dd29384fd598/_apis/wit/classificationNodes/Iterations/Moda%20Integrations%20Team"
                    },
                    {
                        "id": 422,
                        "identifier": "c2d4a31b-9c69-4efb-898e-34a82b559edf",
                        "name": "Team Moda",
                        "structureType": "iteration",
                        "hasChildren": true,
                        "children": [
                            {
                                "id": 423,
                                "identifier": "7b2dca14-11a5-4eec-ac94-68924cbf9342",
                                "name": "2022",
                                "structureType": "iteration",
                                "hasChildren": true,
                                "children": [
                                    {
                                        "id": 424,
                                        "identifier": "4ca97d96-cf9d-484f-b9c8-ee9bb795d167",
                                        "name": "22.3",
                                        "structureType": "iteration",
                                        "hasChildren": true,
                                        "children": [
                                            {
                                                "id": 425,
                                                "identifier": "fe563999-7a47-46b5-b880-18a0daa3f47d",
                                                "name": "22.3.6",
                                                "structureType": "iteration",
                                                "hasChildren": false,
                                                "attributes": {
                                                    "startDate": "2022-09-12T00:00:00Z",
                                                    "finishDate": "2022-09-25T00:00:00Z"
                                                },
                                                "path": "\\Moda\\Iteration\\Team Moda\\2022\\22.3\\22.3.6",
                                                "url": "https://dev.azure.com/dstacey/3b15d01e-d259-48eb-a15c-dd29384fd598/_apis/wit/classificationNodes/Iterations/Team%20Moda/2022/22.3/22.3.6"
                                            },
                                            {
                                                "id": 426,
                                                "identifier": "17edf3d0-b04b-4b60-980a-dfc5ca27c751",
                                                "name": "22.3.7",
                                                "structureType": "iteration",
                                                "hasChildren": false,
                                                "attributes": {
                                                    "startDate": "2022-09-26T00:00:00Z",
                                                    "finishDate": "2022-10-09T00:00:00Z"
                                                },
                                                "path": "\\Moda\\Iteration\\Team Moda\\2022\\22.3\\22.3.7",
                                                "url": "https://dev.azure.com/dstacey/3b15d01e-d259-48eb-a15c-dd29384fd598/_apis/wit/classificationNodes/Iterations/Team%20Moda/2022/22.3/22.3.7"
                                            }
                                        ],
                                        "attributes": {
                                            "startDate": "2022-08-01T00:00:00Z",
                                            "finishDate": "2022-10-09T00:00:00Z"
                                        },
                                        "path": "\\Moda\\Iteration\\Team Moda\\2022\\22.3",
                                        "url": "https://dev.azure.com/dstacey/3b15d01e-d259-48eb-a15c-dd29384fd598/_apis/wit/classificationNodes/Iterations/Team%20Moda/2022/22.3"
                                    },
                                    {
                                        "id": 427,
                                        "identifier": "134aadb6-3666-45ed-bb56-478fa6a12779",
                                        "name": "22.4",
                                        "structureType": "iteration",
                                        "hasChildren": true,
                                        "children": [
                                            {
                                                "id": 428,
                                                "identifier": "5d2df634-b458-4175-8f82-e415640f0472",
                                                "name": "22.4.1",
                                                "structureType": "iteration",
                                                "hasChildren": false,
                                                "attributes": {
                                                    "startDate": "2022-10-10T00:00:00Z",
                                                    "finishDate": "2022-10-23T00:00:00Z"
                                                },
                                                "path": "\\Moda\\Iteration\\Team Moda\\2022\\22.4\\22.4.1",
                                                "url": "https://dev.azure.com/dstacey/3b15d01e-d259-48eb-a15c-dd29384fd598/_apis/wit/classificationNodes/Iterations/Team%20Moda/2022/22.4/22.4.1"
                                            },
                                            {
                                                "id": 429,
                                                "identifier": "5fa3f493-506e-4ebf-b494-8bd3e435b43a",
                                                "name": "22.4.2",
                                                "structureType": "iteration",
                                                "hasChildren": false,
                                                "attributes": {
                                                    "startDate": "2022-10-24T00:00:00Z",
                                                    "finishDate": "2022-11-06T00:00:00Z"
                                                },
                                                "path": "\\Moda\\Iteration\\Team Moda\\2022\\22.4\\22.4.2",
                                                "url": "https://dev.azure.com/dstacey/3b15d01e-d259-48eb-a15c-dd29384fd598/_apis/wit/classificationNodes/Iterations/Team%20Moda/2022/22.4/22.4.2"
                                            },
                                            {
                                                "id": 430,
                                                "identifier": "7aac18ba-8e07-4842-8e9c-fca9af9b837b",
                                                "name": "22.4.3",
                                                "structureType": "iteration",
                                                "hasChildren": false,
                                                "attributes": {
                                                    "startDate": "2022-11-07T00:00:00Z",
                                                    "finishDate": "2022-11-20T00:00:00Z"
                                                },
                                                "path": "\\Moda\\Iteration\\Team Moda\\2022\\22.4\\22.4.3",
                                                "url": "https://dev.azure.com/dstacey/3b15d01e-d259-48eb-a15c-dd29384fd598/_apis/wit/classificationNodes/Iterations/Team%20Moda/2022/22.4/22.4.3"
                                            },
                                        ],
                                        "attributes": {
                                            "startDate": "2022-10-10T00:00:00Z",
                                            "finishDate": "2023-01-01T00:00:00Z"
                                        },
                                        "path": "\\Moda\\Iteration\\Team Moda\\2022\\22.4",
                                        "url": "https://dev.azure.com/dstacey/3b15d01e-d259-48eb-a15c-dd29384fd598/_apis/wit/classificationNodes/Iterations/Team%20Moda/2022/22.4"
                                    }
                                ],
                                "attributes": {
                                    "startDate": "2022-01-03T00:00:00Z",
                                    "finishDate": "2023-01-01T00:00:00Z"
                                },
                                "path": "\\Moda\\Iteration\\Team Moda\\2022",
                                "url": "https://dev.azure.com/dstacey/3b15d01e-d259-48eb-a15c-dd29384fd598/_apis/wit/classificationNodes/Iterations/Team%20Moda/2022"
                            },
                            {
                                "id": 431,
                                "identifier": "95c3321d-e1fd-413b-9506-49fa7427af56",
                                "name": "2023",
                                "structureType": "iteration",
                                "hasChildren": false,
                                "attributes": {
                                    "startDate": "2023-01-02T00:00:00Z",
                                    "finishDate": "2023-12-31T00:00:00Z"
                                },
                                "path": "\\Moda\\Iteration\\Team Moda\\2023",
                                "url": "https://dev.azure.com/dstacey/3b15d01e-d259-48eb-a15c-dd29384fd598/_apis/wit/classificationNodes/Iterations/Team%20Moda/2023"
                            },
                            {
                                "id": 432,
                                "identifier": "e9b7c75f-16ca-4a04-9552-0e41f4f1e0b9",
                                "name": "2024",
                                "structureType": "iteration",
                                "hasChildren": false,
                                "attributes": {
                                    "startDate": "2024-01-01T00:00:00Z",
                                    "finishDate": "2024-12-29T00:00:00Z"
                                },
                                "path": "\\Moda\\Iteration\\Team Moda\\2024",
                                "url": "https://dev.azure.com/dstacey/3b15d01e-d259-48eb-a15c-dd29384fd598/_apis/wit/classificationNodes/Iterations/Team%20Moda/2024"
                            }
                        ],
                        "path": "\\Moda\\Iteration\\Team Moda",
                        "url": "https://dev.azure.com/dstacey/3b15d01e-d259-48eb-a15c-dd29384fd598/_apis/wit/classificationNodes/Iterations/Team%20Moda"
                    }
                ],
                "path": "\\Moda\\Iteration",
                "_links": {
                    "self": {
                        "href": "https://dev.azure.com/dstacey/3b15d01e-d259-48eb-a15c-dd29384fd598/_apis/wit/classificationNodes/Iterations"
                    }
                },
                "url": "https://dev.azure.com/dstacey/3b15d01e-d259-48eb-a15c-dd29384fd598/_apis/wit/classificationNodes/Iterations"
            }
            """;
    }
}
