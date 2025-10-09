using Moda.Planning.Domain.Interfaces.Roadmaps;

namespace Moda.Web.Api.Models.Planning.Roadmaps;

internal record UpsertRoadmapTimeboxDateRangeAdapter : IUpsertRoadmapTimeboxDateRange
{
    public UpsertRoadmapTimeboxDateRangeAdapter(UpdateRoadmapTimeboxDatesRequest request)
    {
        DateRange = new LocalDateRange(request.Start, request.End);
    }

    public LocalDateRange DateRange { get; }
}
