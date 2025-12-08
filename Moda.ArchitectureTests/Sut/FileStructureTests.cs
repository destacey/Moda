using FluentAssertions;
using Moda.ArchitectureTests.Helpers;

namespace Moda.ArchitectureTests.Sut;

/// <summary>
/// Tests to enforce consistent file and folder structure conventions across the solution.
/// These tests ensure that projects are organized according to the modular monolith structure.
///
/// Expected Structure:
///
/// Moda.Common/
///   src/
///     Moda.Common/
///     Moda.Common.Domain/
///     Moda.Common.Application/
///   tests/
///     Moda.Common.Tests/
///     Moda.Common.Domain.Tests/
///     Moda.Common.Application.Tests/
///     Moda.Tests.Shared/
///
/// Moda.Infrastructure/
///   src/
///     Moda.Infrastructure/
///     Moda.Infrastructure.Migrators.MSSQL/
///
/// Moda.Integrations/
///   src/
///     Moda.Integrations.{IntegrationName}/
///   tests/
///     Moda.Integrations.{IntegrationName}.Tests/
///     Moda.Integrations.{IntegrationName}.IntegrationTests/ (optional)
///
/// Moda.Services/
///   Moda.{ServiceName}/
///     src/
///       Moda.{ServiceName}.Domain/
///       Moda.{ServiceName}.Application/
///     tests/
///       Moda.{ServiceName}.Domain.Tests/
///       Moda.{ServiceName}.Application.Tests/
///
/// Moda.Web/
///   src/
///     Moda.Web.Api/
///     moda.web.reactclient/
///
/// Moda.ArchitectureTests/ (exception - no src/tests folders)
///
/// Key Rules:
/// - Source projects MUST be in a 'src' folder
/// - Test projects MUST be in a 'tests' folder
/// - Test projects MUST end with '.Tests' or '.IntegrationTests'
/// - Service projects follow the pattern: Moda.Services/{ServiceName}/src/ and /tests/
/// - Integration projects follow the pattern: Moda.Integrations/src/ and /tests/
/// - Moda.ArchitectureTests is the only exception to the src/tests folder rule
/// </summary>
public class FileStructureTests
{
    private static readonly string SolutionRoot = AssemblyHelper.GetSolutionRoot();

    #region Common Layer Structure Tests

    [Fact]
    public void CommonLayer_SourceProjects_ShouldBeInSrcFolder()
    {
        // Arrange
        var expectedProjects = new[]
        {
            "Moda.Common",
            "Moda.Common.Domain",
            "Moda.Common.Application"
        };

        // Act & Assert
        foreach (var projectName in expectedProjects)
        {
            var projectPath = Path.Combine(SolutionRoot, "Moda.Common", "src", projectName);
            Directory.Exists(projectPath).Should().BeTrue(
                "Common source project {0} should be in Moda.Common/src/ folder", projectName);

            var csprojPath = Path.Combine(projectPath, $"{projectName}.csproj");
            File.Exists(csprojPath).Should().BeTrue(
                "Project file {0} should exist at {1}", $"{projectName}.csproj", csprojPath);
        }
    }

    [Fact]
    public void CommonLayer_TestProjects_ShouldBeInTestsFolder()
    {
        // Arrange
        var expectedTestProjects = new[]
        {
            "Moda.Common.Tests",
            "Moda.Common.Domain.Tests",
            "Moda.Common.Application.Tests",
            "Moda.Tests.Shared"
        };

        // Act & Assert
        foreach (var projectName in expectedTestProjects)
        {
            var projectPath = Path.Combine(SolutionRoot, "Moda.Common", "tests", projectName);
            Directory.Exists(projectPath).Should().BeTrue(
                "Common test project {0} should be in Moda.Common/tests/ folder", projectName);

            var csprojPath = Path.Combine(projectPath, $"{projectName}.csproj");
            File.Exists(csprojPath).Should().BeTrue(
                "Project file {0} should exist at {1}", $"{projectName}.csproj", csprojPath);
        }
    }

    #endregion

    #region Infrastructure Layer Structure Tests

    [Fact]
    public void InfrastructureLayer_SourceProjects_ShouldBeInSrcFolder()
    {
        // Arrange
        var expectedProjects = new[]
        {
            "Moda.Infrastructure",
            "Moda.Infrastructure.Migrators.MSSQL"
        };

        // Act & Assert
        foreach (var projectName in expectedProjects)
        {
            var projectPath = Path.Combine(SolutionRoot, "Moda.Infrastructure", "src", projectName);
            Directory.Exists(projectPath).Should().BeTrue(
                "Infrastructure source project {0} should be in Moda.Infrastructure/src/ folder", projectName);

            var csprojPath = Path.Combine(projectPath, $"{projectName}.csproj");
            File.Exists(csprojPath).Should().BeTrue(
                "Project file {0} should exist at {1}", $"{projectName}.csproj", csprojPath);
        }
    }

    #endregion

    #region Integration Layer Structure Tests

    [Fact]
    public void IntegrationLayer_SourceProjects_ShouldBeInSrcFolder()
    {
        // Arrange
        var integrationsRoot = Path.Combine(SolutionRoot, "Moda.Integrations");
        if (!Directory.Exists(integrationsRoot))
        {
            Assert.Fail("Moda.Integrations directory should exist");
            return;
        }

        var srcFolder = Path.Combine(integrationsRoot, "src");
        if (!Directory.Exists(srcFolder))
        {
            Assert.Fail("Moda.Integrations/src directory should exist");
            return;
        }

        // Act
        var integrationProjects = Directory.GetDirectories(srcFolder)
            .Where(d => Path.GetFileName(d).StartsWith("Moda.Integrations."))
            .ToList();

        // Assert
        integrationProjects.Should().NotBeEmpty("There should be at least one integration project in Moda.Integrations/src/");

        foreach (var projectPath in integrationProjects)
        {
            var projectName = Path.GetFileName(projectPath);
            var csprojPath = Path.Combine(projectPath, $"{projectName}.csproj");
            File.Exists(csprojPath).Should().BeTrue(
                "Integration project {0} should have a .csproj file", projectName);
        }
    }

    [Fact]
    public void IntegrationLayer_TestProjects_ShouldBeInTestsFolder()
    {
        // Arrange
        var integrationsRoot = Path.Combine(SolutionRoot, "Moda.Integrations");
        var testsFolder = Path.Combine(integrationsRoot, "tests");

        if (!Directory.Exists(testsFolder))
        {
            // It's okay if there are no tests yet
            return;
        }

        // Act
        var testProjects = Directory.GetDirectories(testsFolder)
            .Where(d =>
            {
                var name = Path.GetFileName(d);
                return name.StartsWith("Moda.Integrations.") &&
                       (name.EndsWith(".Tests") || name.EndsWith(".IntegrationTests"));
            })
            .ToList();

        // Assert
        foreach (var projectPath in testProjects)
        {
            var projectName = Path.GetFileName(projectPath);
            var csprojPath = Path.Combine(projectPath, $"{projectName}.csproj");
            File.Exists(csprojPath).Should().BeTrue(
                "Integration test project {0} should have a .csproj file", projectName);
        }
    }

    [Fact]
    public void IntegrationLayer_TestProjects_ShouldHaveCorrectSuffix()
    {
        // Arrange
        var integrationsRoot = Path.Combine(SolutionRoot, "Moda.Integrations");
        var testsFolder = Path.Combine(integrationsRoot, "tests");

        if (!Directory.Exists(testsFolder))
        {
            // It's okay if there are no tests yet
            return;
        }

        // Act
        var allTestFolders = Directory.GetDirectories(testsFolder)
            .Where(d => Path.GetFileName(d).StartsWith("Moda.Integrations."))
            .ToList();

        var invalidTestProjects = allTestFolders
            .Where(d =>
            {
                var name = Path.GetFileName(d);
                return !name.EndsWith(".Tests") && !name.EndsWith(".IntegrationTests");
            })
            .Select(Path.GetFileName)
            .ToList();

        // Assert
        invalidTestProjects.Should().BeEmpty(
            "All integration test projects should end with '.Tests' or '.IntegrationTests'. Invalid: {0}",
            string.Join(", ", invalidTestProjects));
    }

    #endregion

    #region Service Layer Structure Tests

    [Fact]
    public void ServiceLayer_AllServices_ShouldHaveSrcFolder()
    {
        // Arrange
        var servicesRoot = Path.Combine(SolutionRoot, "Moda.Services");
        if (!Directory.Exists(servicesRoot))
        {
            Assert.Fail("Moda.Services directory should exist");
            return;
        }

        // Act
        var serviceDirectories = Directory.GetDirectories(servicesRoot)
            .Where(d => Path.GetFileName(d).StartsWith("Moda."))
            .ToList();

        // Assert
        serviceDirectories.Should().NotBeEmpty("There should be at least one service in Moda.Services/");

        var servicesWithoutSrc = serviceDirectories
            .Where(d => !Directory.Exists(Path.Combine(d, "src")))
            .Select(Path.GetFileName)
            .ToList();

        servicesWithoutSrc.Should().BeEmpty(
            "All services should have a 'src' folder. Missing: {0}",
            string.Join(", ", servicesWithoutSrc));
    }

    [Fact]
    public void ServiceLayer_SourceProjects_ShouldBeInSrcFolder()
    {
        // Arrange
        var servicesRoot = Path.Combine(SolutionRoot, "Moda.Services");
        if (!Directory.Exists(servicesRoot))
        {
            Assert.Fail("Moda.Services directory should exist");
            return;
        }

        var serviceDirectories = Directory.GetDirectories(servicesRoot)
            .Where(d => Path.GetFileName(d).StartsWith("Moda."))
            .ToList();

        // Act & Assert
        foreach (var serviceDir in serviceDirectories)
        {
            var serviceName = Path.GetFileName(serviceDir);
            var srcFolder = Path.Combine(serviceDir, "src");

            Directory.Exists(srcFolder).Should().BeTrue(
                "Service {0} should have a src folder", serviceName);

            if (Directory.Exists(srcFolder))
            {
                var sourceProjects = Directory.GetDirectories(srcFolder)
                    .Where(d => Path.GetFileName(d).StartsWith(serviceName))
                    .ToList();

                sourceProjects.Should().NotBeEmpty(
                    "Service {0} should have at least one source project in src/ folder", serviceName);

                foreach (var projectPath in sourceProjects)
                {
                    var projectName = Path.GetFileName(projectPath);
                    var csprojPath = Path.Combine(projectPath, $"{projectName}.csproj");
                    File.Exists(csprojPath).Should().BeTrue(
                        "Service source project {0} should have a .csproj file", projectName);
                }
            }
        }
    }

    [Fact]
    public void ServiceLayer_TestProjects_ShouldBeInTestsFolder()
    {
        // Arrange
        var servicesRoot = Path.Combine(SolutionRoot, "Moda.Services");
        if (!Directory.Exists(servicesRoot))
        {
            Assert.Fail("Moda.Services directory should exist");
            return;
        }

        var serviceDirectories = Directory.GetDirectories(servicesRoot)
            .Where(d => Path.GetFileName(d).StartsWith("Moda."))
            .ToList();

        // Act & Assert
        foreach (var serviceDir in serviceDirectories)
        {
            var serviceName = Path.GetFileName(serviceDir);
            var testsFolder = Path.Combine(serviceDir, "tests");

            if (!Directory.Exists(testsFolder))
            {
                // It's okay if a service doesn't have tests yet (though not recommended)
                continue;
            }

            var testProjects = Directory.GetDirectories(testsFolder)
                .Where(d =>
                {
                    var name = Path.GetFileName(d);
                    return name.StartsWith(serviceName) &&
                           (name.EndsWith(".Tests") || name.EndsWith(".IntegrationTests"));
                })
                .ToList();

            foreach (var projectPath in testProjects)
            {
                var projectName = Path.GetFileName(projectPath);
                var csprojPath = Path.Combine(projectPath, $"{projectName}.csproj");
                File.Exists(csprojPath).Should().BeTrue(
                    "Service test project {0} should have a .csproj file", projectName);
            }
        }
    }

    [Fact]
    public void ServiceLayer_TestProjects_ShouldHaveCorrectSuffix()
    {
        // Arrange
        var servicesRoot = Path.Combine(SolutionRoot, "Moda.Services");
        if (!Directory.Exists(servicesRoot))
        {
            Assert.Fail("Moda.Services directory should exist");
            return;
        }

        var serviceDirectories = Directory.GetDirectories(servicesRoot)
            .Where(d => Path.GetFileName(d).StartsWith("Moda."))
            .ToList();

        var invalidTestProjects = new List<string>();

        // Act
        foreach (var serviceDir in serviceDirectories)
        {
            var serviceName = Path.GetFileName(serviceDir);
            var testsFolder = Path.Combine(serviceDir, "tests");

            if (!Directory.Exists(testsFolder))
            {
                continue;
            }

            var allTestFolders = Directory.GetDirectories(testsFolder)
                .Where(d => Path.GetFileName(d).StartsWith(serviceName))
                .ToList();

            var invalid = allTestFolders
                .Where(d =>
                {
                    var name = Path.GetFileName(d);
                    return !name.EndsWith(".Tests") && !name.EndsWith(".IntegrationTests");
                })
                .Select(Path.GetFileName)
                .Where(name => name != null)
                .Cast<string>()
                .ToList();

            invalidTestProjects.AddRange(invalid);
        }

        // Assert
        invalidTestProjects.Should().BeEmpty(
            "All service test projects should end with '.Tests' or '.IntegrationTests'. Invalid: {0}",
            string.Join(", ", invalidTestProjects));
    }

    #endregion

    #region Web Layer Structure Tests

    [Fact]
    public void WebLayer_SourceProjects_ShouldBeInSrcFolder()
    {
        // Arrange
        var webRoot = Path.Combine(SolutionRoot, "Moda.Web");
        if (!Directory.Exists(webRoot))
        {
            Assert.Fail("Moda.Web directory should exist");
            return;
        }

        var srcFolder = Path.Combine(webRoot, "src");

        // Act & Assert
        Directory.Exists(srcFolder).Should().BeTrue("Moda.Web should have a src folder");

        var expectedProjects = new[]
        {
            "Moda.Web.Api",
            "moda.web.reactclient"
        };

        foreach (var projectName in expectedProjects)
        {
            var projectPath = Path.Combine(srcFolder, projectName);
            Directory.Exists(projectPath).Should().BeTrue(
                "Web project {0} should exist in Moda.Web/src/ folder", projectName);
        }
    }

    #endregion

    #region Architecture Tests Project Exception

    [Fact]
    public void ArchitectureTests_ShouldBeAtRootLevel()
    {
        // Arrange
        var archTestsPath = Path.Combine(SolutionRoot, "Moda.ArchitectureTests");

        // Act & Assert
        Directory.Exists(archTestsPath).Should().BeTrue(
            "Moda.ArchitectureTests should exist at solution root level");

        var csprojPath = Path.Combine(archTestsPath, "Moda.ArchitectureTests.csproj");
        File.Exists(csprojPath).Should().BeTrue(
            "Moda.ArchitectureTests.csproj should exist");
    }

    [Fact]
    public void ArchitectureTests_ShouldNotHaveSrcOrTestsFolders()
    {
        // Arrange
        var archTestsPath = Path.Combine(SolutionRoot, "Moda.ArchitectureTests");

        // Act
        var srcFolder = Path.Combine(archTestsPath, "src");
        var testsFolder = Path.Combine(archTestsPath, "tests");

        // Assert
        Directory.Exists(srcFolder).Should().BeFalse(
            "Moda.ArchitectureTests should not have a 'src' folder - it's the only exception to the src/tests rule");

        Directory.Exists(testsFolder).Should().BeFalse(
            "Moda.ArchitectureTests should not have a 'tests' folder - it's the only exception to the src/tests rule");
    }

    #endregion
}
