namespace Wayd.ArchitectureTests.Helpers;

/// <summary>
/// Provides helper methods for file system operations in architecture tests.
/// This centralizes file/directory discovery logic for test projects and source projects.
/// </summary>
public static class FileSystemHelper
{
    #region Test Project Discovery

    /// <summary>
    /// Gets all test project paths across the solution.
    /// </summary>
    public static List<string> GetAllTestProjectPaths()
    {
        var solutionRoot = AssemblyHelper.GetSolutionRoot();
        var testProjects = new List<string>();

        // Search in Common/tests
        var commonTests = Path.Combine(solutionRoot, "Wayd.Common", "tests");
        if (Directory.Exists(commonTests))
        {
            testProjects.AddRange(Directory.GetFiles(commonTests, "*.csproj", SearchOption.AllDirectories));
        }

        // Search in Services/*/tests
        var servicesRoot = Path.Combine(solutionRoot, "Wayd.Services");
        if (Directory.Exists(servicesRoot))
        {
            var serviceFolders = Directory.GetDirectories(servicesRoot);
            foreach (var serviceFolder in serviceFolders)
            {
                var testsFolder = Path.Combine(serviceFolder, "tests");
                if (Directory.Exists(testsFolder))
                {
                    testProjects.AddRange(Directory.GetFiles(testsFolder, "*.csproj", SearchOption.AllDirectories));
                }
            }
        }

        // Search in Integrations/tests
        var integrationsTests = Path.Combine(solutionRoot, "Wayd.Integrations", "tests");
        if (Directory.Exists(integrationsTests))
        {
            testProjects.AddRange(Directory.GetFiles(integrationsTests, "*.csproj", SearchOption.AllDirectories));
        }

        return testProjects;
    }

    /// <summary>
    /// Gets all service test project paths (from Wayd.Services/*/tests).
    /// </summary>
    public static List<string> GetServiceTestProjects()
    {
        var solutionRoot = AssemblyHelper.GetSolutionRoot();
        var testProjects = new List<string>();
        var servicesRoot = Path.Combine(solutionRoot, "Wayd.Services");

        if (!Directory.Exists(servicesRoot))
        {
            return testProjects;
        }

        var serviceFolders = Directory.GetDirectories(servicesRoot);
        foreach (var serviceFolder in serviceFolders)
        {
            var testsFolder = Path.Combine(serviceFolder, "tests");
            if (Directory.Exists(testsFolder))
            {
                testProjects.AddRange(Directory.GetFiles(testsFolder, "*.csproj", SearchOption.AllDirectories));
            }
        }

        return testProjects;
    }

    /// <summary>
    /// Gets all integration test project paths (from Wayd.Integrations/tests).
    /// </summary>
    public static List<string> GetIntegrationTestProjects()
    {
        var solutionRoot = AssemblyHelper.GetSolutionRoot();
        var testProjects = new List<string>();
        var integrationsTests = Path.Combine(solutionRoot, "Wayd.Integrations", "tests");

        if (!Directory.Exists(integrationsTests))
        {
            return testProjects;
        }

        testProjects.AddRange(Directory.GetFiles(integrationsTests, "*.csproj", SearchOption.AllDirectories));
        return testProjects;
    }

    #endregion

    #region Service Discovery

    /// <summary>
    /// Gets all service directories (Wayd.Services/Wayd.*).
    /// </summary>
    public static List<string> GetServiceDirectories()
    {
        var solutionRoot = AssemblyHelper.GetSolutionRoot();
        var servicesRoot = Path.Combine(solutionRoot, "Wayd.Services");

        if (!Directory.Exists(servicesRoot))
        {
            return [];
        }

        return Directory.GetDirectories(servicesRoot)
            .Where(d => Path.GetFileName(d).StartsWith("Wayd."))
            .ToList();
    }

    /// <summary>
    /// Gets service directories that are actually referenced in the solution file (Wayd.slnx).
    /// This filters out directories that may exist on disk from other branches but aren't part
    /// of the current solution definition.
    /// </summary>
    public static List<string> GetSolutionServiceDirectories()
    {
        var solutionRoot = AssemblyHelper.GetSolutionRoot();
        var slnxPath = Path.Combine(solutionRoot, "Wayd.slnx");
        var slnxContent = File.ReadAllText(slnxPath);

        return GetServiceDirectories()
            .Where(d =>
            {
                var serviceName = Path.GetFileName(d);
                // Check if the solution file references this service (e.g., "Wayd.Services/Wayd.Planning/")
                return slnxContent.Contains($"Wayd.Services/{serviceName}/");
            })
            .ToList();
    }

    #endregion

    #region Folder Structure Helpers

    /// <summary>
    /// Gets the 'src' folder path for a given service.
    /// </summary>
    public static string GetServiceSrcFolder(string serviceDirectory)
    {
        return Path.Combine(serviceDirectory, "src");
    }

    /// <summary>
    /// Gets the 'tests' folder path for a given service.
    /// </summary>
    public static string GetServiceTestsFolder(string serviceDirectory)
    {
        return Path.Combine(serviceDirectory, "tests");
    }

    /// <summary>
    /// Checks if a directory exists and is not empty.
    /// </summary>
    public static bool DirectoryExistsAndNotEmpty(string path)
    {
        return Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any();
    }

    #endregion
}
