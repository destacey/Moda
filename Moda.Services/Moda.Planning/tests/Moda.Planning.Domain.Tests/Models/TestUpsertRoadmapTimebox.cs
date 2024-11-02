using Moda.Common.Models;
using Moda.Planning.Domain.Interfaces.Roadmaps;
using Moda.Planning.Domain.Models.Roadmaps;

namespace Moda.Planning.Domain.Tests.Models;
internal record TestUpsertRoadmapTimebox : TestUpsertRoadmapItem, IUpsertRoadmapTimebox
{
    public TestUpsertRoadmapTimebox(RoadmapTimebox timebox) : base(timebox)
    {
        DateRange = timebox.DateRange;
    }

    public LocalDateRange DateRange { get; set; } = default!;
}
