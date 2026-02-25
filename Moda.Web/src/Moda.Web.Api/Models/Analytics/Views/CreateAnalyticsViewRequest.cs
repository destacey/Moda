using Moda.Analytics.Application.AnalyticsViews.Commands;
using Moda.Analytics.Domain.Enums;
using Moda.Common.Domain.Enums;

namespace Moda.Web.Api.Models.Analytics.Views;

public sealed record CreateAnalyticsViewRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public AnalyticsDataset Dataset { get; set; } = AnalyticsDataset.WorkItems;
    public string DefinitionJson { get; set; } = default!;
    public Visibility Visibility { get; set; } = Visibility.Private;
    public List<Guid> ManagerIds { get; set; } = [];
    public bool IsActive { get; set; } = true;

    public CreateAnalyticsViewCommand ToCreateAnalyticsViewCommand()
        => new(Name, Description, Dataset, DefinitionJson, Visibility, ManagerIds, IsActive);
}

public sealed class CreateAnalyticsViewRequestValidator : CustomValidator<CreateAnalyticsViewRequest>
{
    public CreateAnalyticsViewRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(v => v.Description)
            .MaximumLength(2048);

        RuleFor(v => v.DefinitionJson)
            .NotEmpty();

        RuleFor(v => v.ManagerIds)
            .NotEmpty();

        RuleForEach(v => v.ManagerIds)
            .NotEmpty();
    }
}
