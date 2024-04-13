using System.Text;

namespace Moda.Common.Helpers;

public static class StringHelpers
{
    /// <summary>
    /// Concatenate a list of strings by adding a space in between each word.
    /// </summary>
    /// <param name="words"></param>
    /// <returns>The concatenated string.  Returns an empty string when the array is null or empty.</returns>
    public static string Concat(params string?[] words)
    {
        if (words is null || words.Length == 0)
            return string.Empty;

        var builder = new StringBuilder();

        foreach (var word in words)
        {
            if (!string.IsNullOrWhiteSpace(word))
                builder.Append($"{word} ");
        }

        return builder.ToString().Trim();
    }
}
