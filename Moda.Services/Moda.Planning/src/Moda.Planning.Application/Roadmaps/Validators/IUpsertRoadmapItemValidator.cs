using Moda.Planning.Domain.Interfaces.Roadmaps;

namespace Moda.Planning.Application.Roadmaps.Validators;
public class IUpsertRoadmapItemValidator : CustomValidator<IUpsertRoadmapItem>
{
    public IUpsertRoadmapItemValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(t => t.Description)
            .MaximumLength(2048);

        When(x => x.ParentId.HasValue, () =>
        {
            RuleFor(x => x.ParentId)
                .NotEmpty();
        });

        When(x => x.Color != null, () => RuleFor(x => x.Color)
            .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .WithMessage("Color must be a valid hex color code."));
    }
}
