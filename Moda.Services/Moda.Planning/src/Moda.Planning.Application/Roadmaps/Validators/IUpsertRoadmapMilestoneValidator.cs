using Moda.Planning.Domain.Interfaces.Roadmaps;

namespace Moda.Planning.Application.Roadmaps.Validators;
public sealed class IUpsertRoadmapMilestoneValidator : CustomValidator<IUpsertRoadmapMilestone>
{
    public IUpsertRoadmapMilestoneValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        Include(new IUpsertRoadmapItemValidator());

        RuleFor(x => x.Date)
            .NotNull();
    }
}
