using Wayd.Planning.Domain.Interfaces.Roadmaps;
using Wayd.Planning.Domain.Models.Roadmaps;

namespace Wayd.Planning.Domain.Tests.Models;

internal record TestUpsertRoadmapMilestone : TestUpsertRoadmapItem, IUpsertRoadmapMilestone
{
    public TestUpsertRoadmapMilestone(RoadmapMilestone milestone) : base(milestone)
    {
        Date = milestone.Date;
    }

    public LocalDate Date { get; set; } = default!;
}
