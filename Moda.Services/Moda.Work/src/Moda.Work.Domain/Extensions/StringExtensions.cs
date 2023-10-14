using System.Text.RegularExpressions;
using Moda.Work.Domain.Models;

namespace Moda.Work.Domain.Extensions;
public static class StringExtensions
{
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
