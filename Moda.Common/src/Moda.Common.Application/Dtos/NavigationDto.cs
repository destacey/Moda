namespace Moda.Common.Application.Dtos;

public record NavigationDto
{
    public Guid Id { get; set; }
    public int LocalId { get; set; }
    public required string Name { get; set; }

    public static NavigationDto Create(Guid id, int localId, string name)
        => new()
        {
            Id = id,
            LocalId = localId,
            Name = name
        };
}
