using Ardalis.GuardClauses;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Interfaces.Roadmaps;

namespace Moda.Planning.Domain.Models.Roadmaps;
public sealed class RoadmapTimebox : BaseRoadmapItem
{
    private LocalDateRange _dateRange = default!;

    private RoadmapTimebox() { }

    private RoadmapTimebox(Guid roadmapId, string name, string? description, LocalDateRange dateRange, Guid? parentId, string? color)
    {
        RoadmapId = roadmapId;
        Name = name;
        Description = description;
        Type = RoadmapItemType.Timebox;
        DateRange = dateRange;
        ParentId = parentId;
        Color = color;
    }

    /// <summary>
    /// The date range of the Roadmap Activity.
    /// </summary>
    public LocalDateRange DateRange
    {
        get => _dateRange;
        private set => _dateRange = Guard.Against.Null(value, nameof(DateRange));
    }

    /// <summary>
    /// Creates a new Roadmap Timebox.
    /// </summary>
    /// <param name="roadmapId"></param>
    /// <param name="parentId"></param>
    /// <param name="timebox"></param>
    /// <returns></returns>
    internal static RoadmapTimebox Create(Guid roadmapId, Guid? parentId, IUpsertRoadmapTimebox timebox)
    {
        return new RoadmapTimebox(roadmapId, timebox.Name, timebox.Description, timebox.DateRange, parentId, timebox.Color);
    }
}
