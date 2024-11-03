using Moda.Planning.Domain.Interfaces.Roadmaps;

namespace Moda.Planning.Application.Roadmaps.Validators;
public sealed class IUpsertRoadmapActivityValidator : CustomValidator<IUpsertRoadmapActivity>
{
    public IUpsertRoadmapActivityValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        Include(new IUpsertRoadmapItemValidator());

        RuleFor(x => x.DateRange)
            .NotNull();
    }
}
