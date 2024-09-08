using Moda.Common.Domain.Interfaces;

namespace Moda.Planning.Domain.Models;
public sealed class RoadmapLink : BaseEntity<Guid>, ISystemAuditable
{
    private RoadmapLink() { }
    internal RoadmapLink(Guid parentId, Guid childId, int order)
    {
        ParentId = parentId;
        ChildId = childId;
        Order = order;
    }

    public Guid ParentId { get; private init; }
    public Roadmap? Parent { get; private set; }
    public Guid ChildId { get; private init; }
    public Roadmap? Child { get; private set; }
    public int Order { get; private set; }

    public void SetOrder(int order)
    {
        Order = order;
    }
}
