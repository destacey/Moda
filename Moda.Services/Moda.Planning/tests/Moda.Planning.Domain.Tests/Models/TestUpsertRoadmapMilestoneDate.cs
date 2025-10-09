using System.Diagnostics.CodeAnalysis;
using Moda.Planning.Domain.Interfaces.Roadmaps;

namespace Moda.Planning.Domain.Tests.Models;
internal record TestUpsertRoadmapMilestoneDate : IUpsertRoadmapMilestoneDate
{
    [SetsRequiredMembers]
    public TestUpsertRoadmapMilestoneDate(LocalDate date)
    {
        Date = date;
    }

    public required LocalDate Date { get; set; }
}
