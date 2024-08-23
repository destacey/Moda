namespace Moda.Work.Domain.Models;

// TODO: convert this to an entity value attribute architecture
public sealed class WorkItemExtended
{
    private WorkItemExtended() { }

    private WorkItemExtended(Guid id, string? iterationPath)
    {
        Id = id;
        ExternalTeamIdentifier = iterationPath;
    }

    private WorkItemExtended(string? iterationPath)
        : this(Guid.Empty, iterationPath) // the guid is empty when the work item is not yet created. EF will fill it in.
    { }

    /// <summary>
    /// Work Item Id
    /// </summary>
    public Guid Id { get; init; }

    public string? ExternalTeamIdentifier { get; private set; }

    public void Update(WorkItemExtended? workItemExtended)
    {
        ExternalTeamIdentifier = workItemExtended?.ExternalTeamIdentifier;
    }

    public static WorkItemExtended? Create(string? externalTeamIdentifier)
    {
        return string.IsNullOrWhiteSpace(externalTeamIdentifier) 
            ? null 
            : new WorkItemExtended(externalTeamIdentifier);
    }

    public static WorkItemExtended? Create(Guid id, string? externalTeamIdentifier)
    {
        return string.IsNullOrWhiteSpace(externalTeamIdentifier) 
            ? null 
            : new WorkItemExtended(id, externalTeamIdentifier);
    }
}
