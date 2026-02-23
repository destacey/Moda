using Moda.Analytics.Application.AnalyticsViews.Queries;

namespace Moda.Web.Api.Models.Analytics.Views;

public sealed record RunAnalyticsViewRequest
{
    public Guid Id { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;

    public RunAnalyticsViewQuery ToRunAnalyticsViewQuery()
        => new(Id, PageNumber, PageSize);
}

public sealed class RunAnalyticsViewRequestValidator : CustomValidator<RunAnalyticsViewRequest>
{
    public RunAnalyticsViewRequestValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();

        RuleFor(v => v.PageNumber)
            .GreaterThan(0);

        RuleFor(v => v.PageSize)
            .InclusiveBetween(1, 500);
    }
}
