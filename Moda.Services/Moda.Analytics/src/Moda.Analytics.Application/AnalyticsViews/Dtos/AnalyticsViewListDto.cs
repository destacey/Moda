namespace Moda.Analytics.Application.AnalyticsViews.Dtos;

public sealed record AnalyticsViewListDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public AnalyticsDataset Dataset { get; init; }
    public Visibility Visibility { get; init; }
    public List<Guid> ManagerIds { get; init; } = [];
    public bool IsActive { get; init; }
    public Instant LastModified { get; init; }
}
