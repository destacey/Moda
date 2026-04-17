using NodaTime;

namespace Wayd.Planning.Domain.Interfaces.Roadmaps;

public interface IUpsertRoadmapMilestoneDate
{
    LocalDate Date { get; }
}
