using Ardalis.GuardClauses;
using Moda.Common.Domain.Enums.StrategicManagement;
using Moda.Common.Domain.Interfaces.StrategicManagement;

namespace Moda.ProjectPortfolioManagement.Domain.Models;

/// <summary>
/// Represents a strategic theme within the PPM domain.  The primary StrategicTheme entity is defined in the StrategicManagement domain.
/// </summary>
public class StrategicTheme : IStrategicThemeData, HasIdAndKey
{
    private StrategicTheme() { }

    public StrategicTheme(IStrategicThemeData strategicTheme)
    {
        Guard.Against.Null(strategicTheme, nameof(strategicTheme));
        Guard.Against.NullOrWhiteSpace(strategicTheme.Name, nameof(strategicTheme.Name));
        Guard.Against.NullOrWhiteSpace(strategicTheme.Description, nameof(strategicTheme.Description));

        Id = strategicTheme.Id;
        Key = strategicTheme.Key;
        Name = strategicTheme.Name;
        Description = strategicTheme.Description;
        State = strategicTheme.State;
    }

    public Guid Id { get; private init; }

    public int Key { get; private init; }

    public string Name { get; private set; } = default!;

    public string Description { get; private set; } = default!;

    public StrategicThemeState State { get; private set; }

    /// <summary>
    /// Update the strategic theme with the provided values.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="state"></param>
    public void Update(string name, string description, StrategicThemeState state)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));

        Name = name;
        Description = description;
        State = state;
    }
}
