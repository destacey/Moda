using Moda.Planning.Domain.Interfaces.Roadmaps;
using Moda.Planning.Domain.Models.Roadmaps;

namespace Moda.Planning.Domain.Tests.Models;
internal record class TestUpsertRoadmapItem : IUpsertRoadmapItem
{
    public TestUpsertRoadmapItem(BaseRoadmapItem item)
    {
        Name = item.Name;
        Description = item.Description;
        ParentId = item.ParentId;
        Color = item.Color;
    }

    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public string? Color { get; set; }
}
