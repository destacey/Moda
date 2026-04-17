using Wayd.Common.Models;
using Wayd.Planning.Domain.Interfaces.Roadmaps;
using Wayd.Planning.Domain.Models.Roadmaps;

namespace Wayd.Planning.Domain.Tests.Models;

internal record TestUpsertRoadmapTimebox : TestUpsertRoadmapItem, IUpsertRoadmapTimebox
{
    public TestUpsertRoadmapTimebox(RoadmapTimebox timebox) : base(timebox)
    {
        DateRange = timebox.DateRange;
    }

    public LocalDateRange DateRange { get; set; } = default!;
}
