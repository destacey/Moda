using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Moda.Common.Extensions;

public static class EnumExtensions
{
    public static string GetDisplayName<T>(this T enumValue) where T : IComparable, IFormattable, IConvertible
    {
        ArgumentNullException.ThrowIfNull(enumValue);

        if (!typeof(T).IsEnum)
            throw new ArgumentException("Argument must be of type Enum");

        DisplayAttribute? displayAttribute = enumValue.GetType()
                                                     .GetMember(enumValue.ToString()!)
                                                     .First()
                                                     .GetCustomAttribute<DisplayAttribute>();

        string? displayName = displayAttribute?.GetName();

        return displayName ?? enumValue.ToString()!;
    }
}
