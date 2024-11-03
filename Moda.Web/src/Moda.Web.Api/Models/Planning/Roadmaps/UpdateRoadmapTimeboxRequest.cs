using Moda.Planning.Application.Roadmaps.Commands;
using Moda.Planning.Domain.Interfaces.Roadmaps;
using OneOf;

namespace Moda.Web.Api.Models.Planning.Roadmaps;

public sealed record UpdateRoadmapTimeboxRequest : UpdateRoadmapItemRequest
{
    /// <summary>
    /// The Roadmap Item start date.
    /// </summary>
    public required LocalDate Start { get; set; }

    /// <summary>
    /// The Roadmap Item end date.
    /// </summary>
    public required LocalDate End { get; set; }

    public UpdateRoadmapItemCommand ToUpdateRoadmapItemCommand()
    {
        return new UpdateRoadmapItemCommand(RoadmapId, ItemId, OneOf<IUpsertRoadmapActivity, IUpsertRoadmapMilestone, IUpsertRoadmapTimebox>.FromT2(new UpsertRoadmapTimeboxAdapter(this)));
    }
}

public sealed class UpdateRoadmapTimeboxRequestValidator : CustomValidator<UpdateRoadmapTimeboxRequest>
{
    public UpdateRoadmapTimeboxRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        Include(new UpdateRoadmapItemRequestValidator());

        RuleFor(t => t.Start)
            .NotNull();

        RuleFor(t => t.End)
            .NotNull()
            .Must((membership, end) => membership.Start <= end)
                .WithMessage("End date must be greater than or equal to start date");
    }
}
