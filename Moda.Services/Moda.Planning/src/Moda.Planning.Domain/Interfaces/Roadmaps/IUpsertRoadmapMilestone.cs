using NodaTime;

namespace Moda.Planning.Domain.Interfaces.Roadmaps;

public interface IUpsertRoadmapMilestone : IUpsertRoadmapItem
{
    LocalDate Date { get; }
}
