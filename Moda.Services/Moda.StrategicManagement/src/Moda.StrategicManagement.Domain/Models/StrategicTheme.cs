using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Enums.StrategicManagement;
using Moda.Common.Domain.Events.StrategicManagement;
using Moda.Common.Domain.Interfaces.StrategicManagement;
using NodaTime;

namespace Moda.StrategicManagement.Domain.Models;

/// <summary>
/// Represents a high-level focus area or priority that guides related initiatives.
/// </summary>
public sealed class StrategicTheme : BaseEntity<Guid>, ISystemAuditable, IHasIdAndKey, IStrategicThemeData
{
    private string _name = default!;
    private string _description = default!;

    private StrategicTheme() { }

    private StrategicTheme(string name, string description, StrategicThemeState state)
    {
        Name = name;
        Description = description;
        State = state;
    }

    /// <summary>
    /// The unique key of the StrategicTheme.  This is an alternate key to the Id.
    /// </summary>
    public int Key { get; private init; }

    /// <summary>
    /// The name of the strategic theme, highlighting its focus or priority.
    /// </summary>
    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>
    /// A detailed description of the strategic theme and its importance.
    /// </summary>
    public string Description
    {
        get => _description;
        private set => _description = Guard.Against.NullOrWhiteSpace(value, nameof(Description)).Trim();
    }

    /// <summary>
    /// The current lifecycle state of the strategic theme (e.g., Active, Proposed, Archived).
    /// </summary>
    public StrategicThemeState State { get; private set; }

    /// <summary>
    /// Updates the Strategic Theme.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public Result Update(string name, string description, Instant timestamp)
    {
        Name = name;
        Description = description;

        AddDomainEvent(new StrategicThemeUpdatedEvent(this, timestamp));

        return Result.Success();
    }

    /// <summary>
    /// Activates the Strategic Theme.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public Result Activate(Instant timestamp)
    {
        if (State != StrategicThemeState.Proposed)
        {
            return Result.Failure("Only proposed strategic themes can be activated.");
        }

        State = StrategicThemeState.Active;
        AddDomainEvent(new StrategicThemeUpdatedEvent(this, timestamp));

        return Result.Success();
    }

    /// <summary>
    /// Archives the Strategic Theme.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public Result Archive(Instant timestamp)
    {
        if (State != StrategicThemeState.Active)
        {
            return Result.Failure("Only active strategic themes can be archived.");
        }

        State = StrategicThemeState.Archived;
        AddDomainEvent(new StrategicThemeUpdatedEvent(this, timestamp));

        return Result.Success();
    }

    /// <summary>
    /// Indicates whether the Strategic Theme can be deleted.
    /// </summary>
    /// <returns></returns>
    public bool CanBeDeleted() => State == StrategicThemeState.Proposed;

    /// <summary>
    /// Creates a new Strategic Theme.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="state"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static StrategicTheme Create(string name, string description, StrategicThemeState state, Instant timestamp)
    {
        var theme = new StrategicTheme(name, description, state);

        theme.AddPostPersistenceAction(() => theme.AddDomainEvent(new StrategicThemeCreatedEvent(theme, timestamp)));

        return theme;
    }
}
