namespace Moda.Organization.Application.TeamTypes.Dtos;

public sealed record TeamTypeDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
}
