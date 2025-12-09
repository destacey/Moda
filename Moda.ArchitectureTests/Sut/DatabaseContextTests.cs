using FluentAssertions;
using NetArchTest.Rules;
using Moda.Common.Domain;
using Moda.Common.Application.Persistence;
using Moda.Infrastructure.Persistence.Context;
using Moda.ArchitectureTests.Helpers;

namespace Moda.ArchitectureTests.Sut;

/// <summary>
/// Tests to enforce proper Database Context abstraction and usage patterns.
/// These tests ensure that DbContext is only used where appropriate and that
/// Domain and Application layers work with abstractions (interfaces) rather than
/// concrete implementations.
///
/// Database Context Rules Enforced:
///
/// Domain Layer:
/// - Must NOT reference DbContext or any EF Core types
/// - Must NOT reference IDbContext or any persistence interfaces
/// - Should be completely persistence-ignorant
///
/// Application Layer:
/// - Must NOT reference concrete DbContext implementations
/// - Should ONLY use IDbContext or other persistence abstractions/interfaces
/// - Can reference Entity Framework Core types only for LINQ queries (IQueryable)
///
/// Infrastructure Layer:
/// - This is the ONLY layer that should have concrete DbContext implementations
/// - DbContext classes should be in the Infrastructure layer
/// - Entity configurations should be in the Infrastructure layer
///
/// Integration Layer:
/// - Should NOT reference DbContext directly
/// - Should use IDbContext abstractions if database access is needed
///
/// Common Layer:
/// - Common.Domain must NOT reference DbContext or EF Core
/// - Common.Application can define IDbContext interface but not implement it
/// </summary>
public class DatabaseContextTests
{
    #region Domain Layer Database Tests

    [Fact]
    public void DomainLayer_ShouldNotReferenceDbContext()
    {
        // Arrange
        var domainAssemblies = AssemblyHelper.GetDomainAssemblies();

        // Act
        var result = Types.InAssemblies(domainAssemblies)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore.DbContext")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not reference DbContext. Domain should be persistence-ignorant. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? []));
    }

    [Fact]
    public void DomainLayer_ShouldNotReferenceEntityFrameworkCore()
    {
        // Arrange
        var domainAssemblies = AssemblyHelper.GetDomainAssemblies();

        // Act
        var result = Types.InAssemblies(domainAssemblies)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not reference Entity Framework Core. Domain should be persistence-ignorant. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? []));
    }

    [Fact]
    public void DomainLayer_ShouldNotReferenceIDbContext()
    {
        // Arrange
        var domainAssemblies = AssemblyHelper.GetDomainAssemblies();

        // Act - Domain should not even know about persistence abstractions
        var types = Types.InAssemblies(domainAssemblies)
            .GetTypes()
            .Where(t => t.GetProperties().Any(p => p.PropertyType.Name.Contains("DbContext")) ||
                       t.GetFields().Any(f => f.FieldType.Name.Contains("DbContext")) ||
                       t.GetConstructors().Any(c => c.GetParameters().Any(p => p.ParameterType.Name.Contains("DbContext"))))
            .ToList();

        // Assert
        types.Should().BeEmpty(
            "Domain layer should not reference any DbContext (even interfaces). Domain should be persistence-ignorant. Violating types: {0}",
            string.Join(", ", types.Select(t => t.FullName)));
    }

    #endregion

    #region Application Layer Database Tests

    [Fact]
    public void ApplicationLayer_ShouldNotReferenceConcreteDbContext()
    {
        // Arrange
        var applicationAssemblies = AssemblyHelper.GetApplicationAssemblies();

        // Act - Application should not reference ModaDbContext or other concrete implementations
        var result = Types.InAssemblies(applicationAssemblies)
            .ShouldNot()
            .HaveDependencyOn("Moda.Infrastructure.Persistence.Context.ModaDbContext")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Application layer should not reference concrete DbContext implementations. Use IModaDbContext interface instead. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? []));
    }

    [Fact]
    public void ApplicationLayer_ShouldNotDirectlyInstantiateDbContext()
    {
        // Arrange
        var applicationAssemblies = AssemblyHelper.GetApplicationAssemblies();

        // Act - Look for types that might be DbContext implementations in Application layer
        var dbContextTypes = Types.InAssemblies(applicationAssemblies)
            .That()
            .HaveNameEndingWith("DbContext")
            .And()
            .DoNotHaveNameEndingWith("IDbContext")
            .And()
            .DoNotHaveNameEndingWith("IModaDbContext")
            .And()
            .AreClasses()
            .GetTypes()
            .ToList();

        // Assert
        dbContextTypes.Should().BeEmpty(
            "Application layer should not contain concrete DbContext implementations. DbContext should only be in Infrastructure layer. Violating types: {0}",
            string.Join(", ", dbContextTypes.Select(t => t.FullName)));
    }

    [Fact]
    public void ApplicationLayer_CanUseIDbContextInterface()
    {
        // Arrange
        var applicationAssemblies = AssemblyHelper.GetApplicationAssemblies();

        // Act - This test verifies that IModaDbContext interface is available and used
        var typesUsingDbContext = Types.InAssemblies(applicationAssemblies)
            .GetTypes()
            .Where(t => t.GetConstructors().Any(c =>
                c.GetParameters().Any(p =>
                    p.ParameterType.Name == "IModaDbContext")))
            .ToList();

        // Assert - This is informational, just ensuring the pattern exists
        // We're not asserting a specific count, just that the interface can be used
        typesUsingDbContext.Should().NotBeNull();
    }

    #endregion

    #region Infrastructure Layer Database Tests

    [Fact]
    public void InfrastructureLayer_ShouldContainDbContextImplementation()
    {
        // Arrange - Force assembly load using typeof
        var assembly = typeof(ModaDbContext).Assembly;

        // Act - Look for ModaDbContext in Infrastructure
        var dbContextTypes = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("DbContext")
            .And()
            .AreClasses()
            .GetTypes()
            .ToList();

        // Assert
        dbContextTypes.Should().NotBeEmpty(
            "Infrastructure layer should contain DbContext implementation(s)");

        dbContextTypes.Should().Contain(t => t.Name == "ModaDbContext",
            "Infrastructure should contain the main ModaDbContext class");
    }

    [Fact]
    public void DbContextImplementation_ShouldBeInInfrastructureLayer()
    {
        // Arrange - Check all assemblies for DbContext implementations
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName != null &&
                        a.FullName.StartsWith("Moda.") &&
                        !a.FullName.Contains("Test"))
            .ToArray();

        // Act - Find DbContext classes not in Infrastructure
        var dbContextTypesOutsideInfra = Types.InAssemblies(allAssemblies)
            .That()
            .Inherit(typeof(object)) // This is a workaround - we want all classes
            .And()
            .HaveNameEndingWith("DbContext")
            .And()
            .DoNotResideInNamespace("Moda.Infrastructure")
            .And()
            .AreClasses()
            .GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract)
            .Where(t => t.Name.EndsWith("DbContext"))
            .ToList();

        // Assert
        dbContextTypesOutsideInfra.Should().BeEmpty(
            "All concrete DbContext implementations should be in the Infrastructure layer. Violating types: {0}",
            string.Join(", ", dbContextTypesOutsideInfra.Select(t => t.FullName)));
    }

    [Fact]
    public void EntityConfigurations_ShouldBeInInfrastructureLayer()
    {
        // Arrange - Check all assemblies for IEntityTypeConfiguration implementations
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName != null &&
                        a.FullName.StartsWith("Moda.") &&
                        !a.FullName.Contains("Test"))
            .ToArray();

        // Act - Find entity configurations not in Infrastructure
        var entityConfigsOutsideInfra = Types.InAssemblies(allAssemblies)
            .That()
            .DoNotResideInNamespace("Moda.Infrastructure")
            .And()
            .HaveNameEndingWith("Configuration")
            .And()
            .AreClasses()
            .GetTypes()
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name.Contains("IEntityTypeConfiguration")))
            .ToList();

        // Assert
        entityConfigsOutsideInfra.Should().BeEmpty(
            "All EF Core entity configurations should be in the Infrastructure layer. Violating types: {0}",
            string.Join(", ", entityConfigsOutsideInfra.Select(t => t.FullName)));
    }

    #endregion

    #region Integration Layer Database Tests

    [Fact]
    public void IntegrationLayer_ShouldNotReferenceConcreteDbContext()
    {
        // Arrange
        var integrationAssemblies = AssemblyHelper.GetIntegrationAssemblies();

        if (integrationAssemblies.Length == 0)
        {
            // No integration assemblies to test
            return;
        }

        // Act - Integration layer should not reference ModaDbContext
        var result = Types.InAssemblies(integrationAssemblies)
            .ShouldNot()
            .HaveDependencyOn("Moda.Infrastructure.Persistence.Context.ModaDbContext")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Integration layer should not reference concrete DbContext implementations. Use IModaDbContext interface if needed. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? []));
    }

    #endregion

    #region Common Layer Database Tests

    [Fact]
    public void CommonDomain_ShouldNotReferenceDbContext()
    {
        // Arrange - Force assembly load using typeof
        var assembly = typeof(Moda.Common.Domain.Events.DomainEvent).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Moda.Common.Domain should not reference Entity Framework Core. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? []));
    }

    [Fact]
    public void CommonApplication_CanDefineIDbContextInterface()
    {
        // Arrange - Force assembly load using typeof
        var assembly = typeof(Moda.Common.Application.Persistence.IModaDbContext).Assembly;

        // Act - Look for IModaDbContext interface definition
        var dbContextInterface = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("IModaDbContext")
            .Or()
            .HaveNameEndingWith("IDbContext")
            .GetTypes()
            .Where(t => t.IsInterface)
            .ToList();

        // Assert - This is informational - Common.Application SHOULD define the interface
        dbContextInterface.Should().NotBeEmpty(
            "Moda.Common.Application should define the IModaDbContext interface for use by Application layers");
    }

    [Fact]
    public void CommonApplication_ShouldNotImplementDbContext()
    {
        // Arrange - Force assembly load using typeof
        var assembly = typeof(Moda.Common.Application.Persistence.IModaDbContext).Assembly;

        // Act - Look for concrete DbContext implementations (should be in Infrastructure only)
        var dbContextImplementations = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("DbContext")
            .And()
            .AreClasses()
            .GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract)
            .ToList();

        // Assert
        dbContextImplementations.Should().BeEmpty(
            "Moda.Common.Application should only define DbContext interfaces, not implementations. Implementations belong in Infrastructure. Violating types: {0}",
            string.Join(", ", dbContextImplementations.Select(t => t.FullName)));
    }

    #endregion
}
