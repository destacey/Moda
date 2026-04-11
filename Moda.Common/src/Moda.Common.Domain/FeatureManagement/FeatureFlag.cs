using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Data;

namespace Moda.Common.Domain.FeatureManagement;

public sealed class FeatureFlag : BaseAuditableEntity<int>
{
    private FeatureFlag() { }

    private FeatureFlag(string name, string displayName, string? description, bool isEnabled, bool isSystem)
    {
        Name = name;
        DisplayName = displayName;
        Description = description;
        IsEnabled = isEnabled;
        IsArchived = false;
        IsSystem = isSystem;
    }

    /// <summary>
    /// The programmatic key used in code (e.g., "new-dashboard-layout").
    /// </summary>
    public string Name { get; private init; } = default!;

    /// <summary>
    /// Human-readable name for the admin UI.
    /// </summary>
    public string DisplayName { get; private set; } = default!;

    /// <summary>
    /// Optional description of what this feature flag controls.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Whether this feature is globally enabled.
    /// </summary>
    public bool IsEnabled { get; private set; }

    /// <summary>
    /// Whether this feature flag has been archived (soft removed).
    /// </summary>
    public bool IsArchived { get; private set; }

    /// <summary>
    /// Indicates whether this flag was seeded from code and cannot be archived.
    /// </summary>
    public bool IsSystem { get; private init; }

    /// <summary>
    /// JSON-serialized filter configuration for future targeting support.
    /// </summary>
    public string? FiltersJson { get; private set; }

    public Result Update(string displayName, string? description)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return Result.Failure("Display name is required.");

        DisplayName = displayName.Trim();
        Description = description?.Trim();

        return Result.Success();
    }

    public void Toggle(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }

    public Result Archive()
    {
        if (IsSystem)
            return Result.Failure("System feature flags cannot be archived.");

        if (IsArchived)
            return Result.Failure("Feature flag is already archived.");

        IsArchived = true;
        IsEnabled = false;
        return Result.Success();
    }

    public static Result<FeatureFlag> Create(string name, string displayName, string? description, bool isEnabled, bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<FeatureFlag>("Name is required.");

        if (string.IsNullOrWhiteSpace(displayName))
            return Result.Failure<FeatureFlag>("Display name is required.");

        var trimmedName = name.Trim().ToLowerInvariant();
        if (!IsValidName(trimmedName))
            return Result.Failure<FeatureFlag>("Name must be kebab-case (lowercase letters, numbers, and hyphens only).");

        return Result.Success(new FeatureFlag(trimmedName, displayName.Trim(), description?.Trim(), isEnabled, isSystem));
    }

    private static bool IsValidName(string name) =>
        Regex.IsMatch(name, @"^[a-z0-9]+(-[a-z0-9]+)*$");
}
