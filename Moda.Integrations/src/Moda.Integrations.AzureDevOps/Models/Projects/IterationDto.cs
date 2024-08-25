namespace Moda.Integrations.AzureDevOps.Models.Projects;
internal record IterationDto
{
    public int Id { get; set; }
    public Guid Identifier { get; set; }
    public required string Name { get; set; }
    public Guid? TeamId { get; set; }

    public static Func<IterationNodeDto, IterationDto> FromIterationNodeDto =>
        iterationNodeDto => new IterationDto()
        {
            Id = iterationNodeDto.Id,
            Identifier = iterationNodeDto.Identifier,
            Name = iterationNodeDto.Name,
            TeamId = iterationNodeDto.TeamId
        };
}
