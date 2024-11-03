using Moda.Planning.Application.Roadmaps.Commands;
using Moda.Planning.Domain.Interfaces.Roadmaps;
using OneOf;

namespace Moda.Web.Api.Models.Planning.Roadmaps;

public sealed record UpdateRoadmapMilestoneRequest : UpdateRoadmapItemRequest
{
    /// <summary>
    /// The Milestone date.
    /// </summary>
    public required LocalDate Date { get; set; }

    public UpdateRoadmapItemCommand ToUpdateRoadmapItemCommand()
    {
        return new UpdateRoadmapItemCommand(RoadmapId, ItemId, OneOf<IUpsertRoadmapActivity, IUpsertRoadmapMilestone, IUpsertRoadmapTimebox>.FromT1(new UpsertRoadmapMilestoneAdapter(this)));
    }
}

public sealed class UpdateRoadmapMilestoneRequestValidator : CustomValidator<UpdateRoadmapMilestoneRequest>
{
    public UpdateRoadmapMilestoneRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        Include(new UpdateRoadmapItemRequestValidator());

        RuleFor(t => t.Date)
            .NotNull();
    }
}
