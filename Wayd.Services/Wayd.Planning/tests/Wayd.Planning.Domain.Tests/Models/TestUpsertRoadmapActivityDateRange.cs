using System.Diagnostics.CodeAnalysis;
using Wayd.Common.Models;
using Wayd.Planning.Domain.Interfaces.Roadmaps;

namespace Wayd.Planning.Domain.Tests.Models;

internal record TestUpsertRoadmapActivityDateRange : IUpsertRoadmapActivityDateRange
{
    [SetsRequiredMembers]
    public TestUpsertRoadmapActivityDateRange(LocalDateRange dateRange)
    {
        DateRange = dateRange;
    }

    public required LocalDateRange DateRange { get; set; }
}
