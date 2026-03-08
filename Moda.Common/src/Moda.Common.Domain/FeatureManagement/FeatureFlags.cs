namespace Moda.Common.Domain.FeatureManagement;

/// <summary>
/// Defines all known feature flag names used in the application.
/// Add new entries here when introducing a feature flag.
/// The seeder will automatically create any missing flags on startup.
/// </summary>
public static class FeatureFlags
{
    // Example:
    // public static readonly FeatureFlagDefinition PlanningPoker = new("planning-poker", "Planning Poker", "Controls visibility of the Planning Poker feature.");
}

/// <summary>
/// Represents a feature flag definition to be seeded.
/// </summary>
public sealed record FeatureFlagDefinition(string Name, string DisplayName, string? Description);
