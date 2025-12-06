using FluentAssertions;
using NetArchTest.Rules;

namespace Moda.ArchitectureTests.Sut;

/// <summary>
/// Architecture tests to enforce Clean Architecture dependency rules.
/// These tests ensure that projects only depend on allowed layers according to Clean Architecture principles.
///
/// Dependency Flow (allowed):
/// Domain (innermost) ← Application ← Infrastructure ← Web (outermost)
///
/// Key Rules Enforced:
///
/// Domain Layer (Moda.*.Domain):
/// - Must NOT depend on Application, Infrastructure, Web, or Integrations
/// - Can only depend on Moda.Common and Moda.Common.Domain
/// - Should contain only entities, value objects, domain events, and business logic
///
/// Application Layer (Moda.*.Application):
/// - Must NOT depend on Infrastructure, Web, or Integrations
/// - Should ONLY depend on its own Domain layer (not other service Domain layers)
/// - Can depend on Moda.Common, Moda.Common.Domain, and Moda.Common.Application
/// - Should contain commands, queries, DTOs, and application logic
///
/// Common Layers:
/// - Moda.Common.Domain must NOT depend on Moda.Common.Application
/// - Moda.Common and Moda.Common.Domain must NOT depend on Infrastructure
/// - Moda.Common.Application must NOT depend on Infrastructure
///
/// Infrastructure Layer (Moda.*.Infrastructure):
/// - Must NOT depend on Web layer
/// - Can depend on Application, Domain, and Common layers
///
/// Integration Layer (Moda.Integrations.*):
/// - Must NOT depend on Infrastructure, Web, or any Service layers
/// - Should ONLY depend on Moda.Common layers (Moda.Common, Moda.Common.Domain, Moda.Common.Application)
/// - Should contain external system integration logic (Azure DevOps, Microsoft Graph, etc.)
///
/// Service Isolation:
/// - Each service (Domain and Application projects) should be completely isolated from other services
/// - Service Domain projects must NOT depend on other service Domain projects
/// - Service Application projects must NOT depend on other service Application projects
/// - Service Application projects must NOT depend on other service Domain projects
/// - The Organization.Domain cross-cutting dependency is a known issue that needs to be resolved
/// - All cross-service dependencies should be moved to Moda.Common layers
///
/// Web Layer (not tested here, but allowed to depend on all layers)
/// </summary>
public class CrossProjectDependencyTests
{
    #region Domain Layer Tests

    [Fact]
    public void DomainLayer_ShouldNotDependOnApplication()
    {
        // Arrange
        var domainAssemblies = Types.InAssemblies(GetDomainAssemblies());

        // Act
        var result = domainAssemblies
            .ShouldNot()
            .HaveDependencyOn("*.Application")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not depend on Application layer. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? new List<Type>()));
    }

    [Fact]
    public void DomainLayer_ShouldNotDependOnInfrastructure()
    {
        // Arrange
        var domainAssemblies = Types.InAssemblies(GetDomainAssemblies());

        // Act
        var result = domainAssemblies
            .ShouldNot()
            .HaveDependencyOn("*.Infrastructure")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not depend on Infrastructure layer. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? new List<Type>()));
    }

    [Fact]
    public void DomainLayer_ShouldNotDependOnWeb()
    {
        // Arrange
        var domainAssemblies = Types.InAssemblies(GetDomainAssemblies());

        // Act
        var result = domainAssemblies
            .ShouldNot()
            .HaveDependencyOn("Moda.Web")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not depend on Web layer. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? new List<Type>()));
    }

    [Fact]
    public void DomainLayer_ShouldOnlyDependOnCommonLayers()
    {
        // Arrange
        var domainAssemblies = Types.InAssemblies(GetDomainAssemblies());

        // Act
        var result = domainAssemblies
            .That()
            .ResideInNamespace("Moda")
            .And()
            .DoNotResideInNamespace("Moda.Common.Domain")
            .ShouldNot()
            .HaveDependencyOnAll(
                "Moda.Common.Application",
                "Moda.Infrastructure",
                "Moda.Integrations",
                "Moda.Web")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain projects should only depend on Moda.Common and Moda.Common.Domain (not Application/Infrastructure/Integrations/Web). Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? new List<Type>()));
    }

    #endregion

    #region Application Layer Tests

    [Fact]
    public void ApplicationLayer_ShouldNotDependOnInfrastructure()
    {
        // Arrange
        var applicationAssemblies = Types.InAssemblies(GetApplicationAssemblies());

        // Act
        var result = applicationAssemblies
            .ShouldNot()
            .HaveDependencyOn("*.Infrastructure")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Application layer should not depend on Infrastructure layer. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? new List<Type>()));
    }

    [Fact]
    public void ApplicationLayer_ShouldNotDependOnWeb()
    {
        // Arrange
        var applicationAssemblies = Types.InAssemblies(GetApplicationAssemblies());

        // Act
        var result = applicationAssemblies
            .ShouldNot()
            .HaveDependencyOn("Moda.Web")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Application layer should not depend on Web layer. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? new List<Type>()));
    }

    [Fact]
    public void ApplicationLayer_ShouldNotDependOnIntegrations()
    {
        // Arrange
        var applicationAssemblies = Types.InAssemblies(GetApplicationAssemblies());

        // Act
        var result = applicationAssemblies
            .ShouldNot()
            .HaveDependencyOn("Moda.Integrations")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Application layer should not depend on Integrations projects. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? new List<Type>()));
    }


    #endregion

    #region Infrastructure Layer Tests

    [Fact]
    public void InfrastructureLayer_ShouldNotDependOnWeb()
    {
        // Arrange
        var infrastructureAssemblies = GetInfrastructureAssemblies();

        // Act
        var result = Types.InAssemblies(infrastructureAssemblies)
            .ShouldNot()
            .HaveDependencyOn("Moda.Web")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Infrastructure layer should not depend on Web layer. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? []));
    }

    #endregion

    #region Integration Layer Tests

    [Fact]
    public void IntegrationLayer_ShouldOnlyDependOnCommonLayers()
    {
        // Arrange
        var integrationAssemblies = GetIntegrationAssemblies();

        // Act
        var result = Types.InAssemblies(integrationAssemblies)
            .ShouldNot()
            .HaveDependencyOnAll(
                "Moda.Infrastructure",
                "Moda.Web")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Integration projects should only depend on Moda.Common layers (not Infrastructure, Web, or Service layers). Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? []));
    }

    [Fact]
    public void IntegrationLayer_ShouldNotDependOnServiceDomains()
    {
        // Integration projects should not depend on any service Domain projects
        var integrationAssemblies = GetIntegrationAssemblies();
        var serviceDomains = GetAllServiceDomainAssemblies();
        var failures = new List<string>();

        foreach (var integrationAssembly in integrationAssemblies)
        {
            var serviceDomainNames = serviceDomains
                .Select(a => a.GetName().Name!)
                .ToArray();

            if (serviceDomainNames.Length == 0) continue;

            var types = Types.InAssembly(integrationAssembly);
            var result = types
                .ShouldNot()
                .HaveDependencyOnAny(serviceDomainNames)
                .GetResult();

            if (!result.IsSuccessful)
            {
                var violatingTypes = string.Join(", ", result.FailingTypes ?? []);
                failures.Add($"{integrationAssembly.GetName().Name} -> {violatingTypes}");
            }
        }

        failures.Should().BeEmpty(
            "Integration projects should not depend on service Domain projects. Violations: {0}",
            string.Join("; ", failures));
    }

    [Fact]
    public void IntegrationLayer_ShouldNotDependOnServiceApplications()
    {
        // Integration projects should not depend on any service Application projects
        var integrationAssemblies = GetIntegrationAssemblies();
        var serviceApplications = GetAllServiceApplicationAssemblies();
        var failures = new List<string>();

        foreach (var integrationAssembly in integrationAssemblies)
        {
            var serviceApplicationNames = serviceApplications
                .Select(a => a.GetName().Name!)
                .ToArray();

            if (serviceApplicationNames.Length == 0) continue;

            var types = Types.InAssembly(integrationAssembly);
            var result = types
                .ShouldNot()
                .HaveDependencyOnAny(serviceApplicationNames)
                .GetResult();

            if (!result.IsSuccessful)
            {
                var violatingTypes = string.Join(", ", result.FailingTypes ?? []);
                failures.Add($"{integrationAssembly.GetName().Name} -> {violatingTypes}");
            }
        }

        failures.Should().BeEmpty(
            "Integration projects should not depend on service Application projects. Violations: {0}",
            string.Join("; ", failures));
    }

    #endregion

    #region Common Layer Tests

    [Fact]
    public void CommonDomain_ShouldNotDependOnCommonApplication()
    {
        // Arrange
        var assembly = typeof(Moda.Common.Domain.Events.DomainEvent).Assembly;
        var types = Types.InAssembly(assembly);

        // Act
        var result = types
            .ShouldNot()
            .HaveDependencyOn("Moda.Common.Application")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Moda.Common.Domain should not depend on Moda.Common.Application. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? new List<Type>()));
    }

    [Fact]
    public void CommonDomain_ShouldNotDependOnInfrastructure()
    {
        // Arrange
        var assembly = typeof(Moda.Common.Domain.Events.DomainEvent).Assembly;
        var types = Types.InAssembly(assembly);

        // Act
        var result = types
            .ShouldNot()
            .HaveDependencyOn("*.Infrastructure")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Moda.Common.Domain should not depend on Infrastructure. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? new List<Type>()));
    }

    [Fact]
    public void CommonApplication_ShouldNotDependOnInfrastructure()
    {
        // Arrange
        var assembly = typeof(Moda.Common.Application.Persistence.IModaDbContext).Assembly;
        var types = Types.InAssembly(assembly);

        // Act
        var result = types
            .ShouldNot()
            .HaveDependencyOn("*.Infrastructure")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Moda.Common.Application should not depend on Infrastructure. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? new List<Type>()));
    }

    #endregion

    #region Service Isolation Tests

    [Fact]
    public void ServiceDomains_ShouldNotDependOnOtherServiceDomains()
    {
        // Each service's Domain project should be isolated and not depend on other service Domains
        // This test dynamically discovers all service Domain assemblies

        var allServiceDomains = GetAllServiceDomainAssemblies();
        var failures = new List<string>();

        foreach (var serviceDomain in allServiceDomains)
        {
            // Get all OTHER service domain names (exclude this one)
            var otherServiceDomainNames = allServiceDomains
                .Where(s => s != serviceDomain)
                .Select(a => a.GetName().Name!)
                .ToArray();

            if (otherServiceDomainNames.Length == 0) continue;

            var types = Types.InAssembly(serviceDomain);
            var result = types
                .ShouldNot()
                .HaveDependencyOnAny(otherServiceDomainNames)
                .GetResult();

            if (!result.IsSuccessful)
            {
                var violatingTypes = string.Join(", ", result.FailingTypes ?? []);
                failures.Add($"{serviceDomain.GetName().Name} -> {violatingTypes}");
            }
        }

        failures.Should().BeEmpty(
            "Service Domain projects should not depend on other service Domain projects. Violations: {0}",
            string.Join("; ", failures));
    }

    [Fact]
    public void ServiceApplications_ShouldNotDependOnOtherServiceApplications()
    {
        // Each service's Application project should not depend on other service Application projects
        // This test dynamically discovers all service Application assemblies

        var allServiceApplications = GetAllServiceApplicationAssemblies();
        var failures = new List<string>();

        foreach (var serviceApplication in allServiceApplications)
        {
            // Get all OTHER service application names (exclude this one)
            var otherServiceApplicationNames = allServiceApplications
                .Where(s => s != serviceApplication)
                .Select(a => a.GetName().Name!)
                .ToArray();

            if (otherServiceApplicationNames.Length == 0) continue;

            var types = Types.InAssembly(serviceApplication);
            var result = types
                .ShouldNot()
                .HaveDependencyOnAny(otherServiceApplicationNames)
                .GetResult();

            if (!result.IsSuccessful)
            {
                var violatingTypes = string.Join(", ", result.FailingTypes ?? []);
                failures.Add($"{serviceApplication.GetName().Name} -> {violatingTypes}");
            }
        }

        failures.Should().BeEmpty(
            "Service Application projects should not depend on other service Application projects. Violations: {0}",
            string.Join("; ", failures));
    }

    [Fact]
    public void ServiceApplications_ShouldNotDependOnOtherServiceDomains()
    {
        // Each service's Application project should only depend on its own Domain project
        // This test identifies cross-service Domain dependencies (like the Organization.Domain issue)

        var allServiceApplications = GetAllServiceApplicationAssemblies();
        var allServiceDomains = GetAllServiceDomainAssemblies();
        var failures = new List<string>();

        foreach (var serviceApplication in allServiceApplications)
        {
            // Extract the service name (e.g., "Moda.Work" from "Moda.Work.Application")
            var applicationName = serviceApplication.GetName().Name!;
            var serviceName = applicationName.Replace(".Application", "");
            var ownDomainName = serviceName + ".Domain";

            // Get all OTHER service domain names (exclude own domain)
            var otherServiceDomainNames = allServiceDomains
                .Select(a => a.GetName().Name!)
                .Where(s => s != ownDomainName)
                .ToArray();

            if (otherServiceDomainNames.Length == 0) continue;

            var types = Types.InAssembly(serviceApplication);
            var result = types
                .ShouldNot()
                .HaveDependencyOnAny(otherServiceDomainNames)
                .GetResult();

            if (!result.IsSuccessful)
            {
                var violatingTypes = string.Join(", ", result.FailingTypes ?? []);
                failures.Add($"{applicationName} depends on other service domains -> {violatingTypes}");
            }
        }

        failures.Should().BeEmpty(
            "Service Application projects should only depend on their own Domain project, not other service Domain projects. This test identifies the Organization.Domain cross-cutting dependency issue that needs to be resolved. Violations: {0}",
            string.Join("; ", failures));
    }

    #endregion

    #region Helper Methods

    private static System.Reflection.Assembly[] GetDomainAssemblies()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName != null &&
                        a.FullName.StartsWith("Moda.") &&
                        a.FullName.Contains(".Domain") &&
                        !a.FullName.Contains(".Application") &&
                        !a.FullName.Contains("Test"))
            .ToArray();

        return assemblies;
    }

    private static System.Reflection.Assembly[] GetApplicationAssemblies()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName != null &&
                        a.FullName.StartsWith("Moda.") &&
                        a.FullName.Contains(".Application") &&
                        !a.FullName.Contains("Test"))
            .ToArray();

        return assemblies;
    }

    private static System.Reflection.Assembly[] GetInfrastructureAssemblies()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName != null &&
                        a.FullName.StartsWith("Moda.") &&
                        a.FullName.Contains(".Infrastructure") &&
                        !a.FullName.Contains("Test"))
            .ToArray();

        return assemblies;
    }

    /// <summary>
    /// Dynamically discovers all service Domain assemblies by searching the file system.
    /// Excludes Common.Domain and test assemblies.
    /// Pattern: Moda.{ServiceName}.Domain
    /// </summary>
    private static System.Reflection.Assembly[] GetAllServiceDomainAssemblies()
    {
        var assemblyDir = Path.GetDirectoryName(typeof(CrossProjectDependencyTests).Assembly.Location)!;
        var domainDlls = Directory.GetFiles(assemblyDir, "Moda.*.Domain.dll")
            .Where(f =>
            {
                var fileName = Path.GetFileName(f);
                return !fileName.Contains("Common.Domain") && !fileName.Contains("Test");
            })
            .ToArray();

        return domainDlls.Select(System.Reflection.Assembly.LoadFrom).ToArray();
    }

    /// <summary>
    /// Dynamically discovers all service Application assemblies by searching the file system.
    /// Excludes Common.Application and test assemblies.
    /// Pattern: Moda.{ServiceName}.Application
    /// </summary>
    private static System.Reflection.Assembly[] GetAllServiceApplicationAssemblies()
    {
        var assemblyDir = Path.GetDirectoryName(typeof(CrossProjectDependencyTests).Assembly.Location)!;
        var applicationDlls = Directory.GetFiles(assemblyDir, "Moda.*.Application.dll")
            .Where(f =>
            {
                var fileName = Path.GetFileName(f);
                return !fileName.Contains("Common.Application") && !fileName.Contains("Test");
            })
            .ToArray();

        return applicationDlls.Select(System.Reflection.Assembly.LoadFrom).ToArray();
    }

    /// <summary>
    /// Dynamically discovers all Integration assemblies by searching the file system.
    /// Excludes test assemblies.
    /// Pattern: Moda.Integrations.{IntegrationName}
    /// </summary>
    private static System.Reflection.Assembly[] GetIntegrationAssemblies()
    {
        var assemblyDir = Path.GetDirectoryName(typeof(CrossProjectDependencyTests).Assembly.Location)!;
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
}
