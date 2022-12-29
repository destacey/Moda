using NodaTime;

namespace Moda.Work.Domain.Models;
public class BacklogLevelScheme : BaseAuditableEntity<Guid>
{
    private readonly List<BacklogLevel> _backlogLevels = new();

    private BacklogLevelScheme() { }

    private BacklogLevelScheme(Instant timestamp)
    {
        _backlogLevels.Add(BacklogLevel.Create("Stories", "Stories backlog level.", BacklogCategory.Requirement, Ownership.System, 1, timestamp));

        _backlogLevels.Add(BacklogLevel.Create("Tasks", "Tasks backlog level.", BacklogCategory.Task, Ownership.System, 1, timestamp));

        _backlogLevels.Add(BacklogLevel.Create("Other", "Backlog level for work item types not mapped to one of the primary backlog levels.", BacklogCategory.Other, Ownership.System, 1, timestamp));
        
        _backlogLevels.Add(BacklogLevel.Create("Features", "Features backlog level.", BacklogCategory.Portfolio, Ownership.System, 1, timestamp));
        _backlogLevels.Add(BacklogLevel.Create("Epics", "Epics backlog level.", BacklogCategory.Portfolio, Ownership.System, 2, timestamp));
    }

    public IReadOnlyList<BacklogLevel> BacklogLevels => _backlogLevels.AsReadOnly();

    /// <summary>Initializes the global BacklogLevelScheme.  This should only ever called once.</summary>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns></returns>
    public static BacklogLevelScheme Init(Instant timestamp)
    {
        BacklogLevelScheme scheme = new(timestamp);

        scheme.AddDomainEvent(EntityCreatedEvent.WithEntity(scheme, timestamp));
        return scheme;
    }
}
