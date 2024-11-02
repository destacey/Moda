using Moda.Planning.Domain.Interfaces.Roadmaps;
using Moda.Planning.Domain.Models.Roadmaps;

namespace Moda.Planning.Domain.Tests.Models;
internal record TestUpsertRoadmapMilestone : TestUpsertRoadmapItem, IUpsertRoadmapMilestone
{
    public TestUpsertRoadmapMilestone(RoadmapMilestone milestone) : base(milestone)
    {
        Date = milestone.Date;
    }

    public LocalDate Date { get; set; } = default!;
}
