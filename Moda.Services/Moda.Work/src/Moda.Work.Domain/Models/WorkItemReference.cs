namespace Moda.Work.Domain.Models;
/// <summary>
/// References to work items from other objects.
/// </summary>
public sealed class WorkItemReference : ISystemAuditable
{
    private WorkItemReference() { }
    public WorkItemReference(Guid workItemId, Guid objectId, SystemContext context)
    {
        WorkItemId = workItemId;
        ObjectId = objectId;
        Context = context;
    }

    public Guid WorkItemId { get; private init; }
    public Guid ObjectId { get; private init; }

    /// <summary>
    /// The context of the object referencing the work item.
    /// </summary>
    public SystemContext Context { get; private init; }

    public static WorkItemReference Create(Guid workItemId, Guid objectId, SystemContext context)
    {
        return new WorkItemReference(workItemId, objectId, context);
    }
}
