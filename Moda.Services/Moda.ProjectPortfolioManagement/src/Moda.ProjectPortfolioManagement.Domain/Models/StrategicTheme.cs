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
    /// Update the strategic theme with the provided data
    /// </summary>
    /// <param name="strategicTheme"></param>
    public void Update(IStrategicThemeData strategicTheme)
    {
        Name = strategicTheme.Name;
        Description = strategicTheme.Description;
        State = strategicTheme.State;
    }
}
