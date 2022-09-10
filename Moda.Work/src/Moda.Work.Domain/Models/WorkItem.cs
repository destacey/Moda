namespace Moda.Work.Domain.Models;

public class WorkItem : BaseAuditableEntity<Guid>
{
    private readonly List<WorkItemRevision> _revisions = new();

    private WorkItem() { }

    



    public IReadOnlyCollection<WorkItemRevision> Revisions => _revisions.AsReadOnly();
}
