using System.Text.RegularExpressions;
using Moda.Work.Domain.Models;

namespace Moda.Work.Domain.Extensions;
public static class StringExtensions
{

    /// <summary>
    /// Determines whether the value is valid work item key format.  Work Item keys consist of two groups separated by a hyphen.  The first group is a WorkspaceKey and the second group is a number. Example: "WS-1234"
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    ///   <c>true</c> if [is valid work item key format] [the specified value]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsValidWorkItemKeyFormat(this string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && Regex.IsMatch(value, WorkItemKey.Regex);
    }
}
