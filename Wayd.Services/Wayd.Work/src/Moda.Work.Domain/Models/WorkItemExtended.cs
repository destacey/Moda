namespace Wayd.Work.Domain.Models;

// TODO: convert this to an entity value attribute architecture
public sealed class WorkItemExtended
{
    private WorkItemExtended() { }

    private WorkItemExtended(Guid id, string? iterationPath)
    {
        Id = id;
        ExternalTeamIdentifier = iterationPath;
    }

    /// <summary>
    /// Work Item Id
    /// </summary>
    public Guid Id { get; init; }

    public string? ExternalTeamIdentifier { get; private set; }

    public void Update(WorkItemExtended? workItemExtended)
    {
        ExternalTeamIdentifier = workItemExtended?.ExternalTeamIdentifier;
    }

    public static WorkItemExtended? Create(Guid id, string? externalTeamIdentifier)
    {
        return string.IsNullOrWhiteSpace(externalTeamIdentifier)
            ? null
            : new WorkItemExtended(id, externalTeamIdentifier);
    }
}
