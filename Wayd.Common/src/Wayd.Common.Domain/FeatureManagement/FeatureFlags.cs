namespace Wayd.Common.Domain.FeatureManagement;

/// <summary>
/// Defines all known feature flag names used in the application.
/// Add new entries here when introducing a feature flag.
/// The seeder will automatically create any missing flags on startup.
/// </summary>
public static class FeatureFlags
{
    public static readonly FeatureFlagDefinition PlanningPoker = new(Names.PlanningPoker, "Planning Poker", "Controls visibility of the Planning Poker feature.");

    public static readonly FeatureFlagDefinition IdentityProviders = new(
        Names.IdentityProviders,
        "Identity Providers",
        "Reveals the admin Settings page for managing identity providers (Microsoft Entra ID and generic OIDC). The underlying database-backed provider runtime is always on — this flag only controls whether the management UI is visible to admins.");

    /// <summary>
    /// Compile-time constant names for use in attributes (e.g., [FeatureGate]).
    /// </summary>
    public static class Names
    {
        public const string PlanningPoker = "planning-poker";
        public const string IdentityProviders = "identity-providers";
    }
}

/// <summary>
/// Represents a feature flag definition to be seeded.
/// </summary>
public sealed record FeatureFlagDefinition(string Name, string DisplayName, string? Description);
