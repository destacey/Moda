using Wayd.Planning.Domain.Interfaces.Roadmaps;

namespace Wayd.Web.Api.Models.Planning.Roadmaps;

public sealed record UpsertRoadmapMilestoneDateAdapter : IUpsertRoadmapMilestoneDate
{
    public UpsertRoadmapMilestoneDateAdapter(UpdateRoadmapMilestoneDatesRequest request)
    {
        Date = request.Date;
    }

    public LocalDate Date { get; }
}
