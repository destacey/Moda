namespace Moda.ArchitectureTests.Helpers;

/// <summary>
/// Provides helper methods for loading and accessing assemblies in architecture tests.
/// This centralizes assembly loading logic to ensure consistency across all test classes.
/// </summary>
public static class AssemblyHelper
{
    #region Domain Assembly Methods

    /// <summary>
    /// Gets all Domain assemblies (Moda.*.Domain, including Common.Domain).
    /// </summary>
    public static System.Reflection.Assembly[] GetDomainAssemblies()
    {
        var assemblyDir = GetAssemblyDirectory();
        var domainDlls = Directory.GetFiles(assemblyDir, "Moda.*.Domain.dll")
            .Where(f =>
            {
                var fileName = Path.GetFileName(f);
                return !fileName.Contains("Test");
            })
            .ToArray();

        return domainDlls.Select(System.Reflection.Assembly.LoadFrom).ToArray();
    }

    /// <summary>
    /// Gets all service Domain assemblies (excludes Common.Domain).
    /// Pattern: Moda.{ServiceName}.Domain
    /// </summary>
    public static System.Reflection.Assembly[] GetServiceDomainAssemblies()
    {
        var assemblyDir = GetAssemblyDirectory();
        var domainDlls = Directory.GetFiles(assemblyDir, "Moda.*.Domain.dll")
            .Where(f =>
            {
                var fileName = Path.GetFileName(f);
                return !fileName.Contains("Common.Domain") && !fileName.Contains("Test");
            })
            .ToArray();

        return domainDlls.Select(System.Reflection.Assembly.LoadFrom).ToArray();
    }

    #endregion

    #region Application Assembly Methods

    /// <summary>
    /// Gets all Application assemblies (Moda.*.Application, including Common.Application).
    /// </summary>
    public static System.Reflection.Assembly[] GetApplicationAssemblies()
    {
        var assemblyDir = GetAssemblyDirectory();
        var applicationDlls = Directory.GetFiles(assemblyDir, "Moda.*.Application.dll")
            .Where(f =>
            {
                var fileName = Path.GetFileName(f);
                return !fileName.Contains("Test");
            })
            .ToArray();

        return applicationDlls.Select(System.Reflection.Assembly.LoadFrom).ToArray();
    }

    /// <summary>
    /// Gets all service Application assemblies (excludes Common.Application).
    /// Pattern: Moda.{ServiceName}.Application
    /// </summary>
    public static System.Reflection.Assembly[] GetServiceApplicationAssemblies()
    {
        var assemblyDir = GetAssemblyDirectory();
        var applicationDlls = Directory.GetFiles(assemblyDir, "Moda.*.Application.dll")
            .Where(f =>
            {
                var fileName = Path.GetFileName(f);
                return !fileName.Contains("Common.Application") && !fileName.Contains("Test");
            })
            .ToArray();

        return applicationDlls.Select(System.Reflection.Assembly.LoadFrom).ToArray();
    }

    #endregion

    #region Infrastructure Assembly Methods

    /// <summary>
    /// Gets all Infrastructure assemblies.
    /// </summary>
    public static System.Reflection.Assembly[] GetInfrastructureAssemblies()
    {
        var assemblyDir = GetAssemblyDirectory();
        var infrastructureDlls = Directory.GetFiles(assemblyDir, "Moda.*.Infrastructure*.dll")
            .Where(f =>
            {
                var fileName = Path.GetFileName(f);
                return !fileName.Contains("Test");
            })
            .ToArray();

        return infrastructureDlls.Select(System.Reflection.Assembly.LoadFrom).ToArray();
    }

    #endregion

    #region Integration Assembly Methods

    /// <summary>
    /// Gets all Integration assemblies.
    /// Pattern: Moda.Integrations.{IntegrationName}
    /// </summary>
    public static System.Reflection.Assembly[] GetIntegrationAssemblies()
    {
        var assemblyDir = GetAssemblyDirectory();
        var integrationDlls = Directory.GetFiles(assemblyDir, "Moda.Integrations.*.dll")
            .Where(f =>
            {
                var fileName = Path.GetFileName(f);
                return !fileName.Contains("Test");
            })
            .ToArray();

        return integrationDlls.Select(System.Reflection.Assembly.LoadFrom).ToArray();
    }

    #endregion

    #region Common Assembly Methods

    /// <summary>
    /// Gets the Moda.Common.Domain assembly.
    /// </summary>
    public static System.Reflection.Assembly GetCommonDomainAssembly()
    {
        return typeof(Moda.Common.Domain.Events.DomainEvent).Assembly;
    }

    /// <summary>
    /// Gets the Moda.Common.Application assembly.
    /// </summary>
    public static System.Reflection.Assembly GetCommonApplicationAssembly()
    {
        return typeof(Moda.Common.Application.Persistence.IModaDbContext).Assembly;
    }

    /// <summary>
    /// Gets the Moda.Common assembly.
    /// </summary>
    public static System.Reflection.Assembly GetCommonAssembly()
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "Moda.Common");

        if (assembly == null)
        {
            throw new InvalidOperationException("Moda.Common assembly not found");
        }

        return assembly;
    }

    #endregion

    #region Specific Assembly Methods

    /// <summary>
    /// Gets the Moda.Infrastructure assembly.
    /// </summary>
    public static System.Reflection.Assembly GetInfrastructureAssembly()
    {
        return typeof(Moda.Infrastructure.Persistence.Context.ModaDbContext).Assembly;
    }

    #endregion

    #region File System Helpers

    /// <summary>
    /// Gets the solution root directory.
    /// </summary>
    public static string GetSolutionRoot()
    {
        // Start from the test assembly location and walk up to find the solution root
        var assemblyLocation = GetAssemblyDirectory();
        var currentDir = assemblyLocation;

        while (currentDir != null)
        {
            // Look for Moda.sln file
            if (File.Exists(Path.Combine(currentDir, "Moda.sln")))
            {
                return currentDir;
            }

            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        throw new InvalidOperationException("Could not find solution root. Expected to find Moda.sln file.");
    }

    /// <summary>
    /// Gets the directory where test assemblies are located.
    /// </summary>
    public static string GetAssemblyDirectory()
    {
        var assembly = typeof(AssemblyHelper).Assembly;
        var assemblyLocation = Path.GetDirectoryName(assembly.Location);

        if (string.IsNullOrEmpty(assemblyLocation))
        {
            throw new InvalidOperationException("Could not determine assembly location");
        }

        return assemblyLocation;
    }

    #endregion

    #region Handler Helper Methods

    /// <summary>
    /// Gets the names of all handler types in the given assemblies.
    /// Useful for checking handler dependencies.
    /// </summary>
    public static string[] GetHandlerTypeNames(System.Reflection.Assembly[] assemblies)
    {
        var handlerTypes = NetArchTest.Rules.Types.InAssemblies(assemblies)
            .That()
            .HaveNameEndingWith("Handler")
            .And()
            .AreClasses()
            .GetTypes();

        return handlerTypes
            .Select(t => t.FullName ?? t.Name)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToArray();
    }

    #endregion
}
