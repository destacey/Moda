namespace Moda.Common.Application.Dtos;

public record NavigationDto<TId, TKey>
{
    public NavigationDto() { }

    public NavigationDto(TId id, TKey key, string name)
    {
        Id = id;
        Key = key;
        Name = name;        
    }

    public required TId Id { get; set; }
    public required TKey Key { get; set; }
    public required string Name { get; set; }
}

public record NavigationDto : NavigationDto<Guid, int>
{
    public static NavigationDto Create(Guid id, int key, string name)
    => new()
    {
            Id = id,
            Key = key,
            Name = name
        };
}
