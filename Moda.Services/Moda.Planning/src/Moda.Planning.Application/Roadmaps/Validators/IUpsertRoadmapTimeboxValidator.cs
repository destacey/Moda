using Wayd.Planning.Domain.Interfaces.Roadmaps;

namespace Wayd.Planning.Application.Roadmaps.Validators;

public sealed class IUpsertRoadmapTimeboxValidator : CustomValidator<IUpsertRoadmapTimebox>
{
    public IUpsertRoadmapTimeboxValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        Include(new IUpsertRoadmapItemValidator());

        Include(new IUpsertRoadmapTimeboxDateRangeValidator());
    }
}
