namespace Moda.Analytics.Application.AnalyticsViews.Dtos;

public sealed record AnalyticsViewResultColumnDto(string Field, string DisplayName, string Type);

public sealed record AnalyticsViewResultDto(
    IReadOnlyCollection<AnalyticsViewResultColumnDto> Columns,
    IReadOnlyCollection<Dictionary<string, object?>> Rows,
    int TotalRows);
