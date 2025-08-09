namespace Moda.Integrations.AzureDevOps.Models.Projects;
internal record IterationNodeDto
{
    public int Id { get; set; }

    public Guid Identifier { get; set; }

    public required string Name { get; set; }

    public required string Path { get; set; }

    public Guid? TeamId { get; set; }
    
    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public List<IterationNodeDto>? Children { get; set; }

    public static explicit operator IterationNodeDto(IterationNodeResponse iteration) =>
        new()
        {
            Id = iteration.Id,
            Identifier = iteration.Identifier,
            Name = iteration.Name,
            Path = iteration.Path,
            StartDate = iteration.Attributes?.StartDate,
            EndDate = iteration.Attributes?.EndDate,
            Children = iteration.Children?.Select(x => (IterationNodeDto)x).ToList()
        };
}
