using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Application.Dtos;
public record LifecycleNavigationDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string LifecyclePhase { get; set; }

    public static LifecycleNavigationDto FromEnum<T>(T value) where T : struct, Enum
    {
        return new()
        {
            Id = (int)(object)value,
            Name = value.GetDisplayName(),
            LifecyclePhase = value.GetDisplayGroupName() ?? "Unknown"
        };
    }

    public static LifecycleNavigationDto FromEnum<T>(int value) where T : struct, Enum
    {
        return new()
        {
            Id = value,
            Name = ((T)(object)value).GetDisplayName(),
            LifecyclePhase = ((T)(object)value).GetDisplayGroupName() ?? "Unknown"
        };
    }
}
