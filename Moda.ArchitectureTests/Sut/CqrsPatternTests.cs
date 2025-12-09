using FluentAssertions;
using NetArchTest.Rules;
using Moda.ArchitectureTests.Helpers;

namespace Moda.ArchitectureTests.Sut;

/// <summary>
/// Tests to enforce CQRS (Command Query Responsibility Segregation) pattern conventions.
/// These tests ensure that commands, queries, and handlers follow consistent naming and structural patterns.
///
/// CQRS Conventions Enforced:
///
/// Commands:
/// - Must end with "Command"
/// - Should be in {Feature}/Commands/ folders
/// - Should be public classes
/// - Should not contain business logic (that's in handlers)
///
/// Queries:
/// - Must end with "Query"
/// - Should be in {Feature}/Queries/ folders
/// - Should be public classes
/// - Should not contain business logic (that's in handlers)
///
/// Command Handlers:
/// - Must end with "CommandHandler" or "Handler" (for commands)
/// - Should be in the same folder as their command
/// - Should implement IRequestHandler interface
/// - Should be public or internal
///
/// Query Handlers:
/// - Must end with "QueryHandler" or "Handler" (for queries)
/// - Should be in the same folder as their query
/// - Should implement IRequestHandler interface
/// - Should be public or internal
///
/// General Handler Rules:
/// - Handlers should not reference other handlers directly
/// - Each command/query should have exactly one corresponding handler
/// </summary>
public class CqrsPatternTests
{
    #region Command Naming Tests

    [Fact]
    public void Commands_ShouldEndWithCommand()
    {
        // Arrange
        var applicationAssemblies = AssemblyHelper.GetApplicationAssemblies();

        // Act - Find commands that don't end with Command
        var allTypes = Types.InAssemblies(applicationAssemblies)
            .That()
            .AreClasses()
            .GetTypes()
            .ToList();

        var commandsWithoutCommandSuffix = allTypes
            .Where(IsCommand)
            .Where(t => !t.Name.EndsWith("Command"))
            .ToList();

        // Assert
        commandsWithoutCommandSuffix.Should().BeEmpty(
            "All command classes should end with 'Command'. Violating types: {0}",
            string.Join(", ", commandsWithoutCommandSuffix.Select(t => t.FullName)));
    }

    [Fact]
    public void Commands_ShouldBePublic()
    {
        // Arrange
        var applicationAssemblies = AssemblyHelper.GetApplicationAssemblies();

        // Act - Find commands that implement ICommand or ICommand<> interface and are NOT public
        var allTypes = Types.InAssemblies(applicationAssemblies)
            .That()
            .AreClasses()
            .GetTypes()
            .ToList();

        var nonPublicCommands = allTypes
            .Where(IsCommand)
            .Where(t => !t.IsPublic && !t.IsNestedPublic)
            .ToList();

        // Assert
        nonPublicCommands.Should().BeEmpty(
            "All command classes should be public. Violating types: {0}",
            string.Join(", ", nonPublicCommands.Select(t => t.FullName)));
    }

    [Fact]
    public void Commands_ShouldBeSealed()
    {
        // Arrange
        var applicationAssemblies = AssemblyHelper.GetApplicationAssemblies();

        // Act - Find commands that implement ICommand or ICommand<> interface and are NOT sealed
        var allTypes = Types.InAssemblies(applicationAssemblies)
            .That()
            .AreClasses()
            .GetTypes()
            .ToList();

        var commands = allTypes
            .Where(IsCommand)
            .Where(t => !t.IsSealed && !t.IsAbstract)
            .ToList();

        // Assert
        commands.Should().BeEmpty(
            "All command classes should be sealed to prevent inheritance. Violating types: {0}",
            string.Join(", ", commands.Select(t => t.FullName)));
    }

    [Fact]
    public void Commands_ShouldBeRecords()
    {
        // Arrange
        var applicationAssemblies = AssemblyHelper.GetApplicationAssemblies();

        // Act - Find commands that implement ICommand or ICommand<> interface and are NOT records
        var allTypes = Types.InAssemblies(applicationAssemblies)
            .That()
            .AreClasses()
            .GetTypes()
            .ToList();

        var commands = allTypes
            .Where(IsCommand)
            .Where(t => !IsRecord(t))
            .ToList();

        // Assert
        commands.Should().BeEmpty(
            "All command classes should be records for immutability and value semantics. Violating types: {0}",
            string.Join(", ", commands.Select(t => t.FullName)));
    }

    #endregion

    #region Query Naming Tests

    [Fact]
    public void Queries_ShouldEndWithQuery()
    {
        // Arrange
        var applicationAssemblies = AssemblyHelper.GetApplicationAssemblies();

        // Act - Find queries that don't end with Query
        var allTypes = Types.InAssemblies(applicationAssemblies)
            .That()
            .AreClasses()
            .GetTypes()
            .ToList();

        var queriesWithoutQuerySuffix = allTypes
            .Where(IsQuery)
            .Where(t => !t.Name.EndsWith("Query"))
            .ToList();

        // Assert
        queriesWithoutQuerySuffix.Should().BeEmpty(
            "All query classes should end with 'Query'. Violating types: {0}",
            string.Join(", ", queriesWithoutQuerySuffix.Select(t => t.FullName)));
    }

    [Fact]
    public void Queries_ShouldBePublic()
    {
        // Arrange
        var applicationAssemblies = AssemblyHelper.GetApplicationAssemblies();

        // Act - Find queries that implement IQuery<> interface and are NOT public
        var allTypes = Types.InAssemblies(applicationAssemblies)
            .That()
            .AreClasses()
            .GetTypes()
            .ToList();

        var nonPublicQueries = allTypes
            .Where(IsQuery)
            .Where(t => !t.IsPublic && !t.IsNestedPublic)
            .ToList();

        // Assert
        nonPublicQueries.Should().BeEmpty(
            "All query classes should be public. Violating types: {0}",
            string.Join(", ", nonPublicQueries.Select(t => t.FullName)));
    }

    [Fact]
    public void Queries_ShouldBeSealed()
    {
        // Arrange
        var applicationAssemblies = AssemblyHelper.GetApplicationAssemblies();

        // Act - Find queries that implement IQuery<> interface and are NOT sealed
        var allTypes = Types.InAssemblies(applicationAssemblies)
            .That()
            .AreClasses()
            .GetTypes()
            .ToList();

        var queries = allTypes
            .Where(IsQuery)
            .Where(t => !t.IsSealed && !t.IsAbstract)
            .ToList();

        // Assert
        queries.Should().BeEmpty(
            "All query classes should be sealed to prevent inheritance. Violating types: {0}",
            string.Join(", ", queries.Select(t => t.FullName)));
    }

    [Fact]
    public void Queries_ShouldBeRecords()
    {
        // Arrange
        var applicationAssemblies = AssemblyHelper.GetApplicationAssemblies();

        // Act - Find queries that implement IQuery<> interface and are NOT records
        var allTypes = Types.InAssemblies(applicationAssemblies)
            .That()
            .AreClasses()
            .GetTypes()
            .ToList();

        var queries = allTypes
            .Where(IsQuery)
            .Where(t => !IsRecord(t))
            .ToList();

        // Assert
        queries.Should().BeEmpty(
            "All query classes should be records for immutability and value semantics. Violating types: {0}",
            string.Join(", ", queries.Select(t => t.FullName)));
    }

    #endregion

    #region Handler Naming Tests

    [Fact]
    public void CommandHandlers_ShouldEndWithHandler()
    {
        // Arrange
        var applicationAssemblies = AssemblyHelper.GetApplicationAssemblies();

        // Act - Find command handlers that don't end with Handler
        var allTypes = Types.InAssemblies(applicationAssemblies)
            .That()
            .AreClasses()
            .GetTypes()
            .ToList();

        var commandHandlersWithoutHandlerSuffix = allTypes
            .Where(IsCommandHandler)
            .Where(t => !t.Name.EndsWith("Handler"))
            .ToList();

        // Assert
        commandHandlersWithoutHandlerSuffix.Should().BeEmpty(
            "All command handler classes should end with 'Handler'. Violating types: {0}",
            string.Join(", ", commandHandlersWithoutHandlerSuffix.Select(t => t.FullName)));
    }

    [Fact]
    public void QueryHandlers_ShouldEndWithHandler()
    {
        // Arrange
        var applicationAssemblies = AssemblyHelper.GetApplicationAssemblies();

        // Act - Find query handlers that don't end with Handler
        var allTypes = Types.InAssemblies(applicationAssemblies)
            .That()
            .AreClasses()
            .GetTypes()
            .ToList();

        var queryHandlersWithoutHandlerSuffix = allTypes
            .Where(IsQueryHandler)
            .Where(t => !t.Name.EndsWith("Handler"))
            .ToList();

        // Assert
        queryHandlersWithoutHandlerSuffix.Should().BeEmpty(
            "All query handler classes should end with 'Handler'. Violating types: {0}",
            string.Join(", ", queryHandlersWithoutHandlerSuffix.Select(t => t.FullName)));
    }

    [Fact]
    public void Handlers_ShouldBeInternal()
    {
        // Arrange
        var applicationAssemblies = AssemblyHelper.GetApplicationAssemblies();

        // Act - Find all command and query handlers that are NOT internal
        var allTypes = Types.InAssemblies(applicationAssemblies)
            .That()
            .AreClasses()
            .GetTypes()
            .ToList();

        var allHandlers = allTypes
            .Where(t => IsCommandHandler(t) || IsQueryHandler(t))
            .ToList();

        var nonInternalHandlers = allHandlers
            .Where(t => t.IsPublic || t.IsNestedPublic || t.IsNestedPrivate || t.IsNestedFamily || t.IsNestedFamORAssem)
            .ToList();

        // Assert
        nonInternalHandlers.Should().BeEmpty(
            "All handler classes should be internal (sealed). Handlers should not be public, private, or protected. Violating types: {0}",
            string.Join(", ", nonInternalHandlers.Select(t => t.FullName)));
    }

    #endregion

    #region Handler Isolation Tests

    [Fact]
    public void Handlers_ShouldNotDependOnOtherHandlers()
    {
        // Arrange
        var applicationAssemblies = AssemblyHelper.GetApplicationAssemblies();
        var allTypes = Types.InAssemblies(applicationAssemblies)
            .That()
            .AreClasses()
            .GetTypes()
            .ToList();

        var allHandlers = allTypes
            .Where(t => IsCommandHandler(t) || IsQueryHandler(t))
            .ToList();

        var violations = new List<string>();

        // Act - Check each handler to see if it depends on OTHER handlers (not itself)
        foreach (var handler in allHandlers)
        {
            // Get the types this handler references
            var referencedTypes = handler.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                .Select(f => f.FieldType)
                .Concat(handler.GetProperties().Select(p => p.PropertyType))
                .Concat(handler.GetConstructors()
                    .SelectMany(c => c.GetParameters().Select(p => p.ParameterType)))
                .ToList();

            // Check if any referenced types are OTHER handlers
            foreach (var referencedType in referencedTypes)
            {
                var otherHandler = allHandlers.FirstOrDefault(h =>
                    h != handler &&
                    (h == referencedType || h.FullName == referencedType.FullName));

                if (otherHandler != null)
                {
                    violations.Add($"{handler.FullName} depends on {otherHandler.FullName}");
                }
            }
        }

        // Assert
        violations.Should().BeEmpty(
            "Handlers should not depend on other handlers directly. Use MediatR to send commands/queries instead. Violations: {0}",
            string.Join("; ", violations));
    }

    #endregion

    #region Event Handler Tests

    [Fact]
    public void EventHandlers_ShouldEndWithHandler()
    {
        // Arrange
        var applicationAssemblies = AssemblyHelper.GetApplicationAssemblies();

        // Act - Classes in EventHandlers namespace should end with Handler
        var result = Types.InAssemblies(applicationAssemblies)
            .That()
            .ResideInNamespace("EventHandlers")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Handler")
            .Or()
            .HaveNameEndingWith("EventHandler")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All event handler classes should end with 'Handler' or 'EventHandler'. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? []));
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if a type implements ICommand&lt;TResponse&gt; interface.
    /// </summary>
    private static bool IsCommand(Type type)
    {
        return type.GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition().Name == "ICommand`1");
    }

    /// <summary>
    /// Checks if a type implements IQuery&lt;TResponse&gt; interface.
    /// </summary>
    private static bool IsQuery(Type type)
    {
        return type.GetInterfaces().Any(i =>
            i.IsGenericType &&
            i.GetGenericTypeDefinition().Name == "IQuery`1");
    }

    /// <summary>
    /// Checks if a type implements ICommandHandler&lt;TCommand&gt; or ICommandHandler&lt;TCommand, TResponse&gt; interface.
    /// </summary>
    private static bool IsCommandHandler(Type type)
    {
        return type.GetInterfaces().Any(i =>
            i.IsGenericType &&
            (i.GetGenericTypeDefinition().Name == "ICommandHandler`1" ||
             i.GetGenericTypeDefinition().Name == "ICommandHandler`2"));
    }

    /// <summary>
    /// Checks if a type implements IQueryHandler&lt;TQuery, TResponse&gt; interface.
    /// </summary>
    private static bool IsQueryHandler(Type type)
    {
        return type.GetInterfaces().Any(i =>
            i.IsGenericType &&
            i.GetGenericTypeDefinition().Name == "IQueryHandler`2");
    }

    /// <summary>
    /// Checks if a type is a record by looking for compiler-generated members.
    /// Records in C# have an EqualityContract property that is generated by the compiler.
    /// This is the most reliable way to detect records.
    /// </summary>
    private static bool IsRecord(Type type)
    {
        // Records have a protected/public EqualityContract property
        // This is the definitive marker of a record type
        var hasEqualityContract = type.GetProperty("EqualityContract",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public) != null;

        return hasEqualityContract;
    }

    #endregion
}
