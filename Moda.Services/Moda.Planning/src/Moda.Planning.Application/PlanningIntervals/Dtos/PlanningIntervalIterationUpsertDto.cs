using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.PlanningIntervals.Dtos;

public sealed record PlanningIntervalIterationUpsertDto(Guid? IterationId, string Name, IterationType Type, LocalDateRange DateRange);

public sealed class PlanningIntervalIterationUpsertDtoValidator : CustomValidator<PlanningIntervalIterationUpsertDto>
{
    public PlanningIntervalIterationUpsertDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(c => c.Type)
            .IsInEnum()
            .WithMessage(errorMessage: "A valid iteration type must be selected."); ;

        RuleFor(c => c.DateRange)
            .NotNull();
    }
}
