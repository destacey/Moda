using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Moda.Common.Extensions;

public static class EnumExtensions
{
    public static string GetDisplayName<T>(this T enumValue) where T : IComparable, IFormattable, IConvertible
    {
        DisplayAttribute? displayAttribute = GetDisplayAttribute<T>(enumValue);
        return displayAttribute?.GetName() ?? enumValue.ToString()!;
    }

    public static string? GetDisplayDescription<T>(this T enumValue) where T : IComparable, IFormattable, IConvertible
    {
        DisplayAttribute? displayAttribute = GetDisplayAttribute<T>(enumValue);
        return displayAttribute?.GetDescription();
    }

    public static int GetDisplayOrder<T>(this T enumValue) where T : IComparable, IFormattable, IConvertible
    {
        DisplayAttribute? displayAttribute = GetDisplayAttribute<T>(enumValue);
        return displayAttribute?.GetOrder() ?? 999;
    }

    public static string? GetDisplayGroupName<T>(this T enumValue) where T : IComparable, IFormattable, IConvertible
    {
        DisplayAttribute? displayAttribute = GetDisplayAttribute<T>(enumValue);
        return displayAttribute?.GetGroupName();
    }

    private static DisplayAttribute? GetDisplayAttribute<T>(T enumValue)
    {
        ArgumentNullException.ThrowIfNull(enumValue);

        return typeof(T).IsEnum
            ? enumValue.GetType()
                        .GetMember(enumValue.ToString()!)
                        .First()
                        .GetCustomAttribute<DisplayAttribute>()
            : throw new ArgumentException("Argument must be of type Enum");
    }
}
