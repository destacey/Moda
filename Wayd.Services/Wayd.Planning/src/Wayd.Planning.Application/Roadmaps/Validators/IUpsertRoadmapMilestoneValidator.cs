using Wayd.Planning.Domain.Interfaces.Roadmaps;

namespace Wayd.Planning.Application.Roadmaps.Validators;

public sealed class IUpsertRoadmapMilestoneValidator : CustomValidator<IUpsertRoadmapMilestone>
{
    public IUpsertRoadmapMilestoneValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        Include(new IUpsertRoadmapItemValidator());

        Include(new IUpsertRoadmapMilestoneDateValidator());
    }
}
