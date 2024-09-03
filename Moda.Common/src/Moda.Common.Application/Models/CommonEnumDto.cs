namespace Moda.Common.Application.Models;
public record CommonEnumDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "Unknown Name";
    public string? Description { get; set; }
    public int Order { get; set; }

    public static List<TType> GetValues<TEnum, TType>() where TEnum : struct, Enum where TType : CommonEnumDto, new()
    {
        return Enum.GetValues<TEnum>().Select(v => new TType
        {
            Id = (int)(object)v,
            Name = v.GetDisplayName(),
            Description = v.GetDisplayDescription(),
            Order = v.GetDisplayOrder()
        }).ToList();
    }
}