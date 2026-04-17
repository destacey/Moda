using Wayd.Planning.Domain.Interfaces.Roadmaps;

namespace Wayd.Web.Api.Models.Planning.Roadmaps;

internal record UpsertRoadmapActivityDateRangeAdapter : IUpsertRoadmapActivityDateRange
{
    public UpsertRoadmapActivityDateRangeAdapter(UpdateRoadmapActivityDatesRequest request)
    {
        DateRange = new LocalDateRange(request.Start, request.End);
    }

    public LocalDateRange DateRange { get; }
}
