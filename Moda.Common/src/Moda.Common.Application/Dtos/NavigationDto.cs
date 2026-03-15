using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Application.Dtos;

public record DescriptiveNavigationDto<TId, TKey>
{
    public DescriptiveNavigationDto() { }

    public DescriptiveNavigationDto(TId id, TKey key, string name, string? description)
    {
        Id = id;
        Key = key;
        Name = name;
        Description = description;
    }

    [Required]
    public required TId Id { get; set; }

    [Required]
    public required TKey Key { get; set; }

    [Required]
    public required string Name { get; set; }

    public string? Description { get; set; }
}

public record DescriptiveNavigationDto : DescriptiveNavigationDto<Guid, int>
{
    public static DescriptiveNavigationDto Create(Guid id, int key, string name, string? description)
    => new()
    {
            Id = id,
            Key = key,
            Name = name,
            Description = description
    };
}
