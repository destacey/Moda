using Moda.Planning.Domain.Interfaces.Roadmaps;

namespace Moda.Web.Api.Models.Planning.Roadmaps;

public sealed record UpsertRoadmapMilestoneDateAdapter : IUpsertRoadmapMilestoneDate
{
    public UpsertRoadmapMilestoneDateAdapter(UpdateRoadmapMilestoneDatesRequest request)
    {
        Date = request.Date;
    }

    public LocalDate Date { get; }
}
