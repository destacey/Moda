namespace Moda.Planning.Domain.Interfaces.Roadmaps;

public interface IUpsertRoadmapActivity : IUpsertRoadmapItem
{
    LocalDateRange DateRange { get; }
}
