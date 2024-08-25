using System.Text.Json.Serialization;

namespace Moda.Integrations.AzureDevOps.Models.Projects;
internal record IterationNodeDto
{
    public int Id { get; set; }

    public Guid Identifier { get; set; }

    public required string Name { get; set; }

    public Guid? TeamId { get; set; }

    public List<IterationNodeDto>? Children { get; set; }

    public static explicit operator IterationNodeDto(ClassificationNodeResponse classificationNodeResponse) =>
        new()
        {
            Id = classificationNodeResponse.Id,
            Identifier = classificationNodeResponse.Identifier,
            Name = classificationNodeResponse.Name,
            Children = classificationNodeResponse.Children?.Select(x => (IterationNodeDto)x).ToList()
        };
}
