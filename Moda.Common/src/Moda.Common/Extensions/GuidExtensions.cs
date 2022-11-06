namespace Moda.Common.Extensions;
public static class GuidExtensions
{
    public static bool IsDefault(this Guid value)
        => value == default;

    public static bool IsDefault(this Guid? value)
        => value is null ? false : value.Value.IsDefault();
}
