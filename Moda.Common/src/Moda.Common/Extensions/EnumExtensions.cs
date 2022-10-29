using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Moda.Common.Extensions;

public static class EnumExtensions
{
    public static string GetDisplayName<T>(this T enumValue) where T : IComparable, IFormattable, IConvertible
    {
        DisplayAttribute? displayAttribute = GetDisplayAttribute<T>(enumValue);
        string? displayName = displayAttribute?.GetName();

        return displayName ?? enumValue.ToString()!;
    }

    public static string? GetDisplayDescription<T>(this T enumValue) where T : IComparable, IFormattable, IConvertible
    {
        DisplayAttribute? displayAttribute = GetDisplayAttribute<T>(enumValue);
        return displayAttribute?.GetDescription();
    }

    private static DisplayAttribute? GetDisplayAttribute<T>(T enumValue)
    {
        ArgumentNullException.ThrowIfNull(enumValue);

        if (!typeof(T).IsEnum)
            throw new ArgumentException("Argument must be of type Enum");

        return enumValue.GetType()
                        .GetMember(enumValue.ToString()!)
                        .First()
                        .GetCustomAttribute<DisplayAttribute>();
    }
}
