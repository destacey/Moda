namespace Moda.Analytics.Application.AnalyticsViews.Dtos;

public sealed record AnalyticsViewColumnDto(string Field, string DisplayName);

public sealed record AnalyticsViewDataResultDto(
    IReadOnlyList<AnalyticsViewColumnDto> Columns,
    IReadOnlyList<Dictionary<string, object?>> Rows,
    int TotalCount);
