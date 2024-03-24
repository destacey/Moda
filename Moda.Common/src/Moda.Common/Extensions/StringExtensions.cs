using System.Text.RegularExpressions;
using Moda.Common.Models;

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

    /// <summary>
    /// Determines whether the value is valid workspace key format.  Workspace keys are uppercase letters and numbers only, must start with an uppercase letter, and 2-20 characters.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    ///   <c>true</c> if [is valid workspace key format] [the specified value]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsValidWorkspaceKeyFormat(this string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && Regex.IsMatch(value, WorkspaceKey.Regex);
    }
}
