namespace Moda.Planning.Domain.Interfaces.Roadmaps;

public interface IUpsertRoadmapTimebox
{
    string Name { get; }
    string? Description { get; }
    LocalDateRange DateRange { get; }
    string? Color { get; }
}
