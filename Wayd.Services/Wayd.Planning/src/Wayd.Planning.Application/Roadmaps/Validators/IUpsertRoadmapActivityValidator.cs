using Wayd.Planning.Domain.Interfaces.Roadmaps;

namespace Wayd.Planning.Application.Roadmaps.Validators;

public sealed class IUpsertRoadmapActivityValidator : CustomValidator<IUpsertRoadmapActivity>
{
    public IUpsertRoadmapActivityValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        Include(new IUpsertRoadmapItemValidator());

        Include(new IUpsertRoadmapActivityDateRangeValidator());
    }
}
