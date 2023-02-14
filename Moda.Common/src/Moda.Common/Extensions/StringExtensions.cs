using System.Text.RegularExpressions;

namespace Moda.Common.Extensions;

public static class StringExtensions
{
    public static bool IsValidEmailAddressFormat(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;

        string emailAddressRegex = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";

        return Regex.IsMatch(value, emailAddressRegex, RegexOptions.IgnoreCase);
    }

    public static bool IsValidPhoneNumberFormat(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;

        string phoneNumberRegex = "^[0-9]{10}$";

        return Regex.IsMatch(value, phoneNumberRegex);
    }

    public static bool IsValidZipCodeFormat(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;

        string zipCodeRegex = "^[0-9]{5}(?:-[0-9]{4})?$";

        return Regex.IsMatch(value, zipCodeRegex);
    }

    public static string? NullIfWhiteSpacePlusTrim(this string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
