using NodaTime;

namespace Moda.Planning.Domain.Interfaces.Roadmaps;
public interface IUpsertRoadmapMilestoneDate
{
    LocalDate Date { get; }
}
