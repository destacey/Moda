using Wayd.Planning.Domain.Interfaces.Roadmaps;

namespace Wayd.Planning.Application.Roadmaps.Validators;

public sealed class IUpsertRoadmapTimeboxDateRangeValidator : CustomValidator<IUpsertRoadmapTimeboxDateRange>
{
    public IUpsertRoadmapTimeboxDateRangeValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.DateRange)
            .NotNull();
    }
}
