using System.Text.RegularExpressions;
using Moda.Common.Domain.Models.Organizations;

namespace Moda.Common.Domain.Extensions.Organizations;
public static partial class StringExtensions
{
    /// <summary>
    /// Determines whether the value is valid team code format.  Team codes are uppercase letters and numbers only, 2-10 characters.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    ///   <c>true</c> if [is valid team code format] [the specified value]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsValidTeamCodeFormat(this string? value)
    {
        // TODO: move to Moda.Common.Domain

        return !string.IsNullOrWhiteSpace(value)
            && TeamCodeRegex().IsMatch(value);
    }

    [GeneratedRegex(TeamCode.Regex)]
    private static partial Regex TeamCodeRegex();
}
