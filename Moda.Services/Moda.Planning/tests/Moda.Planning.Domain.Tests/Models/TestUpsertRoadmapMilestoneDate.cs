using System.Diagnostics.CodeAnalysis;
using Wayd.Planning.Domain.Interfaces.Roadmaps;

namespace Wayd.Planning.Domain.Tests.Models;

internal record TestUpsertRoadmapMilestoneDate : IUpsertRoadmapMilestoneDate
{
    [SetsRequiredMembers]
    public TestUpsertRoadmapMilestoneDate(LocalDate date)
    {
        Date = date;
    }

    public required LocalDate Date { get; set; }
}
