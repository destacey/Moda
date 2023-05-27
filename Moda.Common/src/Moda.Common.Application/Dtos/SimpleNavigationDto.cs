namespace Moda.Common.Application.Dtos;
public record SimpleNavigationDto
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public static SimpleNavigationDto FromEnum<T>(T value) where T : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        return new()
        {
            Id = (int)(object)value,
            Name = value.GetDisplayName()
        };
    }

    public static SimpleNavigationDto FromEnum<T>(int value) where T : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        return new()
        {
            Id = value,
            Name = ((T)(object)value).GetDisplayName()
        };
    }
}
