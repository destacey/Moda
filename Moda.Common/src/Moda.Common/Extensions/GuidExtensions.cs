namespace Moda.Common.Extensions;
public static class GuidExtensions
{
    public static bool IsDefault(this Guid value)
        => value == default;

    public static bool IsDefault(this Guid? value)
        => value is not null && value.Value.IsDefault();

    public static bool IsNullEmptyOrDefault(this Guid? value)
        => value is null || value.Value == Guid.Empty || value.Value.IsDefault();
}
