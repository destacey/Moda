namespace Moda.Common.Application.Dtos;

public record NavigationDto
{
    public Guid Id { get; set; }
    public int Key { get; set; }
    public required string Name { get; set; }

    public static NavigationDto Create(Guid id, int key, string name)
        => new()
        {
            Id = id,
            Key = key,
            Name = name
        };
}
