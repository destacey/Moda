using Moda.Planning.Application.Roadmaps.Commands;
using Moda.Planning.Domain.Interfaces.Roadmaps;
using OneOf;

namespace Moda.Web.Api.Models.Planning.Roadmaps;

public sealed record CreateRoadmapActivityRequest : CreateRoadmapItemRequest
{
    /// <summary>
    /// The Activity start date.
    /// </summary>
    public LocalDate Start { get; set; }

    /// <summary>
    /// The Activity end date.
    /// </summary>
    public LocalDate End { get; set; }

    public CreateRoadmapItemCommand ToCreateRoadmapItemCommand()
    {
        return new CreateRoadmapItemCommand(RoadmapId, OneOf<IUpsertRoadmapActivity, IUpsertRoadmapMilestone, IUpsertRoadmapTimebox>.FromT0(new UpsertRoadmapActivityAdapter(this)));
    }
}

public sealed class CreateRoadmapActivityRequestValidator : CustomValidator<CreateRoadmapActivityRequest>
{
    public CreateRoadmapActivityRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        Include(new CreateRoadmapItemRequestValidator());

        RuleFor(t => t.Start)
            .NotNull();

        RuleFor(t => t.End)
            .NotNull()
            .Must((membership, end) => membership.Start <= end)
                .WithMessage("End date must be greater than or equal to start date");
    }
}
