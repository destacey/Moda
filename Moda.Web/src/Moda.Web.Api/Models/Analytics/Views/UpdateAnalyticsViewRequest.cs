using Moda.Analytics.Application.AnalyticsViews.Commands;
using Moda.Analytics.Domain.Enums;
using Moda.Common.Domain.Enums;

namespace Moda.Web.Api.Models.Analytics.Views;

public sealed record UpdateAnalyticsViewRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public AnalyticsDataset Dataset { get; set; } = AnalyticsDataset.WorkItems;
    public string DefinitionJson { get; set; } = default!;
    public Visibility Visibility { get; set; } = Visibility.Private;
    public List<Guid> ManagerIds { get; set; } = [];
    public bool IsActive { get; set; } = true;

    public UpdateAnalyticsViewCommand ToUpdateAnalyticsViewCommand()
        => new(Id, Name, Description, Dataset, DefinitionJson, Visibility, ManagerIds, IsActive);
}

public sealed class UpdateAnalyticsViewRequestValidator : CustomValidator<UpdateAnalyticsViewRequest>
{
    public UpdateAnalyticsViewRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(v => v.Id)
            .NotEmpty();

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
