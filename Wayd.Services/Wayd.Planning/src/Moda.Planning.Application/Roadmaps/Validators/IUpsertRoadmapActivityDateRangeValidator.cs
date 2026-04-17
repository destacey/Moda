using Wayd.Planning.Domain.Interfaces.Roadmaps;

namespace Wayd.Planning.Application.Roadmaps.Validators;

public sealed class IUpsertRoadmapActivityDateRangeValidator : CustomValidator<IUpsertRoadmapActivityDateRange>
{
    public IUpsertRoadmapActivityDateRangeValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.DateRange)
            .NotNull();
    }
}
