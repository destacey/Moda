using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Application.Dtos;
public record SimpleNavigationDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }

    public static SimpleNavigationDto Create(int id, string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return new()
        {
            Id = id,
            Name = name
        };
    }

    public static SimpleNavigationDto FromEnum<T>(T value) where T : struct, Enum
    {
        return new()
        {
            Id = (int)(object)value,
            Name = value.GetDisplayName()
        };
    }

    public static SimpleNavigationDto FromEnum<T>(int value) where T : struct, Enum
    {
        return new()
        {
            Id = value,
            Name = ((T)(object)value).GetDisplayName()
        };
    }
}
