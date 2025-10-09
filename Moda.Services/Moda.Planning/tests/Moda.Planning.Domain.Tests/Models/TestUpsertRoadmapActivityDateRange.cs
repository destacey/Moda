using System.Diagnostics.CodeAnalysis;
using Moda.Common.Models;
using Moda.Planning.Domain.Interfaces.Roadmaps;

namespace Moda.Planning.Domain.Tests.Models;
internal record TestUpsertRoadmapActivityDateRange : IUpsertRoadmapActivityDateRange
{
    [SetsRequiredMembers]
    public TestUpsertRoadmapActivityDateRange(LocalDateRange dateRange)
    {
        DateRange = dateRange;
    }

    public required LocalDateRange DateRange { get; set; }
}
