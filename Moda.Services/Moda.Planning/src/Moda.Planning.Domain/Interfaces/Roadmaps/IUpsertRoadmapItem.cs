namespace Wayd.Planning.Domain.Interfaces.Roadmaps;

public interface IUpsertRoadmapItem
{
    string Name { get; }
    string? Description { get; }
    Guid? ParentId { get; }
    string? Color { get; }
}
