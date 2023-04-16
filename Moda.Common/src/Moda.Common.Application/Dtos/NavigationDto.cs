namespace Moda.Common.Application.Dtos;

public record NavigationDto
{
    public Guid Id { get; set; }
    public int LocalId { get; set; }
    public required string Name { get; set; }
}
