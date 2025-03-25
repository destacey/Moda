using Moda.Planning.Application.PlanningIntervals.Dtos;
using Moda.Planning.Domain.Enums;

namespace Moda.Web.Api.Models.Planning.PlanningIntervals;

public sealed record PlanningIntervalIterationUpsertRequest
{
    public Guid? IterationId { get; set; }

    /// <summary>
    /// The name of the iteration.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// The type of iteration.
    /// </summary>
    public int TypeId { get; set; }

    /// <summary>Gets or sets the start.</summary>
    /// <value>The start.</value>
    public LocalDate Start { get; set; }

    /// <summary>Gets or sets the end.</summary>
    /// <value>The end.</value>
    public LocalDate End { get; set; }

    public PlanningIntervalIterationUpsertDto ToPlanningIntervalIterationUpsertDto()
    {
        return new PlanningIntervalIterationUpsertDto(IterationId, Name, (IterationType)TypeId, new LocalDateRange(Start, End));
    }
}

public sealed class PlanningIntervalIterationUpsertRequestValidator : CustomValidator<PlanningIntervalIterationUpsertRequest>
{
    public PlanningIntervalIterationUpsertRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(i => i.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(c => (IterationType)c.TypeId)
            .IsInEnum()
            .WithMessage(errorMessage: "A valid iteration type must be selected."); ;

        RuleFor(i => i.Start)
            .NotNull();

        RuleFor(i => i.End)
            .NotNull()
            .Must((iteration, end) => iteration.Start <= end)
                .WithMessage("End date must be greater than or equal to start date");
    }
}