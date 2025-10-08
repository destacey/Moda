using Moda.Planning.Domain.Interfaces.Roadmaps;

namespace Moda.Planning.Application.Roadmaps.Validators;
public sealed class IUpsertRoadmapActivityDateRangeValidator : CustomValidator<IUpsertRoadmapActivityDateRange>
{
    public IUpsertRoadmapActivityDateRangeValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.DateRange)
            .NotNull();
    }
}
