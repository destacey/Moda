using System.Text.RegularExpressions;

namespace Moda.Organization.Domain.Extensions;
public static class StringExtensions
{
    /// <summary>
    /// Determines whether the value is valid organization code format.  Organiation codes are uppercase letters only, 2-10 characters.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    ///   <c>true</c> if [is valid organization code format] [the specified value]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsValidOrganizationCodeFormat(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;

        string regex = "^([A-Z]){2,10}$";

        return Regex.IsMatch(value, regex);
    }
}
