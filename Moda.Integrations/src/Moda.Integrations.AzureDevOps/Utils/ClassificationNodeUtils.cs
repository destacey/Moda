namespace Moda.Integrations.AzureDevOps.Utils;

/// <summary>
/// Utility methods for working with Azure DevOps classification nodes.
/// </summary>
internal static class ClassificationNodeUtils
{
    /// <summary>
    /// Removes the classification type (second node) from an Azure DevOps classification path.
    /// The classification type is always the second node in the path (e.g., "Iteration", "Area").
    /// </summary>
    /// <remarks>
    /// Azure DevOps classification paths follow the format: \\{ProjectName}\\{ClassificationType}\\{Hierarchy...}
    /// This method removes the classification type to return just the project and hierarchy.
    ///
    /// Examples:
    /// - "\\Moda\\Iteration\\Team Moda" -> "\\Moda\\Team Moda"
    /// - "\\Moda\\Area\\Product\\Feature" -> "\\Moda\\Product\\Feature"
    /// - "\\Moda\\Iteration" -> "\\Moda"
    /// </remarks>
    /// <param name="path">The classification path from Azure DevOps.</param>
    /// <returns>The path with the classification type node removed.</returns>
    public static string RemoveClassificationTypeFromPath(string path)
    {
        // Find the second backslash (start of classification type node)
        int firstBackslash = path.IndexOf('\\');
        if (firstBackslash == -1) 
            return path;

        int secondBackslash = path.IndexOf('\\', firstBackslash + 1);
        if (secondBackslash == -1) 
            return path;

        // Find the third backslash (end of classification type node)
        int thirdBackslash = path.IndexOf('\\', secondBackslash + 1);

        if (thirdBackslash == -1)
        {
            // Path is just '\\Project\\Type' - return '\\Project'
            return path[..secondBackslash];
        }

        // Combine project name with the rest of the hierarchy
        // Uses Span<char> to minimize allocations for performance
        return string.Concat(
            path.AsSpan(0, secondBackslash),
            path.AsSpan(thirdBackslash)
        );
    }
}
