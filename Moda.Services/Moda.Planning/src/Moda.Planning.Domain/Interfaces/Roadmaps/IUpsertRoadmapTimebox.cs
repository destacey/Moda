namespace Moda.Planning.Domain.Interfaces.Roadmaps;

public interface IUpsertRoadmapTimebox : IUpsertRoadmapItem
{
    LocalDateRange DateRange { get; }
}
