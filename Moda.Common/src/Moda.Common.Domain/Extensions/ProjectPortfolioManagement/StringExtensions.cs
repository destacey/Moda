using System.Text.RegularExpressions;
using Moda.Common.Domain.Models.ProjectPortfolioManagement;

namespace Moda.Common.Domain.Extensions.ProjectPortfolioManagement;
public static partial class StringExtensions
{
    /// <summary>
    /// Determines whether the value is valid project key format. Project keys are uppercase letters and numbers only, 2-20 characters.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    ///   <c>true</c> if [is valid project key format] [the specified value]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsValidProjectKeyFormat(this string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && ProjectKeyRegex().IsMatch(value);
    }

    [GeneratedRegex(ProjectKey.Regex)]
    private static partial Regex ProjectKeyRegex();
}
