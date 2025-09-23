using Moda.Planning.Domain.Interfaces.Roadmaps;

namespace Moda.Planning.Application.Roadmaps.Validators;
public sealed class IUpsertRoadmapTimeboxValidator : CustomValidator<IUpsertRoadmapTimebox>
{
    public IUpsertRoadmapTimeboxValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        Include(new IUpsertRoadmapItemValidator());

        Include(new IUpsertRoadmapTimeboxDateRangeValidator());
    }
}
