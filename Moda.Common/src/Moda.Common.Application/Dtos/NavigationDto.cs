using System.ComponentModel.DataAnnotations;
using Moda.Common.Domain.Interfaces;

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

    [Required]
    public required TId Id { get; set; }

    [Required]
    public required TKey Key { get; set; }

    [Required]
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

    /// <summary>
    /// Creates a NavigationDto from any entity implementing INavigable.
    /// </summary>
    public static NavigationDto FromNavigable<T>(T entity) where T : INavigable
    {
        return new NavigationDto
        {
            Id = entity.Id,
            Key = entity.Key,
            Name = entity.Name
        };
    }
}
