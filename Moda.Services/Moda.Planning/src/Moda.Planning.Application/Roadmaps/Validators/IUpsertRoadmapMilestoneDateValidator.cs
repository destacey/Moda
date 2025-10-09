using Moda.Planning.Domain.Interfaces.Roadmaps;

namespace Moda.Planning.Application.Roadmaps.Validators;
public sealed class IUpsertRoadmapMilestoneDateValidator : CustomValidator<IUpsertRoadmapMilestoneDate>
{
    public IUpsertRoadmapMilestoneDateValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Date)
            .NotNull();
    }
}
