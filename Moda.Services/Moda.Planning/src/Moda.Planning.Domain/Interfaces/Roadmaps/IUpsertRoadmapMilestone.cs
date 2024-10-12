using NodaTime;

namespace Moda.Planning.Domain.Interfaces.Roadmaps;

public interface IUpsertRoadmapMilestone
{
    string Name { get; }
    string? Description { get; }
    LocalDate Date { get; }
    string? Color { get; }
}
