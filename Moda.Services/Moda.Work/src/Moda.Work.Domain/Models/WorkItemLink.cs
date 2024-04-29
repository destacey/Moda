namespace Moda.Work.Domain.Models;
public sealed class WorkItemLink : ISystemAuditable
{
    private WorkItemLink() { }
    public WorkItemLink(Guid workItemId, Guid objectId, SystemContext context)
    {
        WorkItemId = workItemId;
        ObjectId = objectId;
        Context = context;
    }

    public Guid WorkItemId { get; private init; }
    public Guid ObjectId { get; private init; }

    /// <summary>
    /// The context of the object being linked to the work item.
    /// </summary>
    public SystemContext Context { get; private init; }

    public static WorkItemLink Create(Guid workItemId, Guid objectId, SystemContext context)
    {
        return new WorkItemLink(workItemId, objectId, context);
    }
}
