using Moda.Planning.Application.Roadmaps.Commands;

namespace Moda.Web.Api.Models.Planning.Roadmaps;

public sealed record CreateRoadmapItemRequest
{
    /// <summary>
    /// The Roadmap Id the Roadmap Item belongs to.
    /// </summary>
    public Guid RoadmapId { get; set; }

    /// <summary>
    /// The name of the Roadmap Item.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The description of the Roadmap Item.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The parent Roadmap Item Id. This is used to connect Roadmap Items together.
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// The Roadmap Item start date.
    /// </summary>
    public required LocalDate Start { get; set; }

    /// <summary>
    /// The Roadmap Item end date.
    /// </summary>
    public required LocalDate End { get; set; }

    /// <summary>
    /// The color of the Roadmap Item. This is used to display the Roadmap Item in the UI.
    /// </summary>
    public string? Color { get; set; }

    public CreateRoadmapItemCommand ToCreateRoadmapItemCommand()
    {
        return new CreateRoadmapItemCommand(RoadmapId, Name, Description, ParentId, new LocalDateRange(Start, End), Color);
    }
}

public sealed class CreateRoadmapItemRequestValidator : CustomValidator<CreateRoadmapItemRequest>
{
    public CreateRoadmapItemRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.RoadmapId)
            .NotEmpty();

        RuleFor(t => t.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(t => t.Description)
            .MaximumLength(2048);

        When(x => x.ParentId.HasValue, () =>
        {
            RuleFor(x => x.ParentId)
                .NotEmpty();
        });

        RuleFor(t => t.Start)
            .NotNull();

        RuleFor(t => t.End)
            .NotNull()
            .Must((membership, end) => membership.Start <= end)
                .WithMessage("End date must be greater than or equal to start date");

        When(x => x.Color != null, () => RuleFor(x => x.Color)
            .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .WithMessage("Color must be a valid hex color code."));
    }
}
