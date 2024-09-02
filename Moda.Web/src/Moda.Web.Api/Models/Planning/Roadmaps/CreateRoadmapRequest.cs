using Moda.Planning.Application.Roadmaps.Commands;

namespace Moda.Web.Api.Models.Planning.Roadmaps;

public sealed record CreateRoadmapRequest
{
    /// <summary>
    /// The name of the Roadmap.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The description of the Roadmap.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The Roadmap start date.
    /// </summary>
    public LocalDate Start { get; set; }

    /// <summary>
    /// The Roadmap end date.
    /// </summary>
    public LocalDate End { get; set; }

    /// <summary>
    /// Indicates if the Roadmap is public.  If true, the Roadmap is visible to all users. If false, the Roadmap is only visible to the managers.
    /// </summary>
    public bool IsPublic { get; set; }

    public CreateRoadmapCommand ToCreateRoadmapCommand()
    {
        return new CreateRoadmapCommand(Name, Description, new LocalDateRange(Start, End), IsPublic);
    }
}

public sealed class CreateRoadmapRequestValidator : CustomValidator<CreateRoadmapRequest>
{
    public CreateRoadmapRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(t => t.Description)
            .MaximumLength(2048);

        RuleFor(t => t.Start)
            .NotNull();

        RuleFor(t => t.End)
            .NotNull()
            .Must((membership, end) => membership.Start <= end)
                .WithMessage("End date must be greater than or equal to start date");
    }
}
