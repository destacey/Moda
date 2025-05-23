﻿using Moda.Common.Domain.Enums;
using Moda.Planning.Application.Roadmaps.Commands;

namespace Moda.Web.Api.Models.Planning.Roadmaps;

public sealed record CreateRoadmapRequest
{
    /// <summary>
    /// The name of the Roadmap.
    /// </summary>
    public string Name { get; set; } = default!;

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
    /// The managers of the Roadmap.
    /// </summary>
    public List<Guid> RoadmapManagerIds { get; set; } = default!;

    /// <summary>
    /// The visibility id for the Roadmap. If the Roadmap is public, all users can see the Roadmap. Otherwise, only the Roadmap Managers can see the Roadmap.
    /// </summary>
    public int VisibilityId { get; set; }

    public CreateRoadmapCommand ToCreateRoadmapCommand()
    {
        return new CreateRoadmapCommand(Name, Description, new LocalDateRange(Start, End), RoadmapManagerIds, (Visibility)VisibilityId);
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
            .Must((roadmap, end) => roadmap.Start <= end)
                .WithMessage("End date must be greater than or equal to start date");

        RuleFor(t => t.RoadmapManagerIds)
            .NotEmpty();

        RuleForEach(t => t.RoadmapManagerIds)
            .NotEmpty();

        RuleFor(t => (Visibility)t.VisibilityId)
            .IsInEnum()
            .WithMessage("A valid visibility must be selected.");
    }
}
