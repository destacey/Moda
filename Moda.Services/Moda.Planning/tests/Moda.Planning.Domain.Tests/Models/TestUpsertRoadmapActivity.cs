using Moda.Common.Models;
using Moda.Planning.Domain.Interfaces.Roadmaps;
using Moda.Planning.Domain.Models.Roadmaps;

namespace Moda.Planning.Domain.Tests.Models;
internal record TestUpsertRoadmapActivity : TestUpsertRoadmapItem, IUpsertRoadmapActivity
{
    public TestUpsertRoadmapActivity(RoadmapActivity timebox) : base(timebox)
    {
        DateRange = timebox.DateRange;
    }

    public LocalDateRange DateRange { get; set; } = default!;
}
