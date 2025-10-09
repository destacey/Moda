using System.Diagnostics.CodeAnalysis;
using Moda.Common.Models;
using Moda.Planning.Domain.Interfaces.Roadmaps;

namespace Moda.Planning.Domain.Tests.Models;
internal record TestUpsertRoadmapTimeboxDateRange : IUpsertRoadmapTimeboxDateRange
{
    [SetsRequiredMembers]
    public TestUpsertRoadmapTimeboxDateRange(LocalDateRange dateRange)
    {
        DateRange = dateRange;
    }

    public required LocalDateRange DateRange { get; set; }
}
