﻿using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.PlanningIntervals.Dtos;
public sealed record ImportPlanningIntervalObjectiveDto(
    int ImportId,
    Guid PlanningIntervalId,
    Guid TeamId,
    string Name,
    string? Description,
    ObjectiveStatus Status,
    double Progress,
    LocalDate? StartDate,
    LocalDate? TargetDate,
    bool IsStretch,
    Instant? ClosedDateUtc,
    int? Order);

public sealed class ImportPlanningIntervalObjectiveDtoValidator : CustomValidator<ImportPlanningIntervalObjectiveDto>
{
    public ImportPlanningIntervalObjectiveDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(o => o.TeamId)
            .NotEmpty()
            .WithMessage("A plan must be selected.");

        RuleFor(o => o.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(o => o.Description)
            .MaximumLength(1024);

        RuleFor(o => o.Status)
            .IsInEnum()
            .WithMessage("A valid status must be selected.");

        RuleFor(o => o.Progress)
            .InclusiveBetween(0.0d, 100.0d)
            .WithMessage("The progress must be between 0 and 100.");

        When(o => o.StartDate.HasValue && o.TargetDate.HasValue, () =>
        {
            RuleFor(o => o.StartDate)
                .LessThan(o => o.TargetDate)
                .WithMessage("The start date must be before the target date.");
        });

        When(o => o.Status is ObjectiveStatus.Completed or ObjectiveStatus.Canceled or ObjectiveStatus.Missed,
            () => RuleFor(o => o.ClosedDateUtc)
                .NotEmpty()
                    .WithMessage("The ClosedDateUtc can not be empty if the status is Completed or Canceled."))
            .Otherwise(() => RuleFor(o => o.ClosedDateUtc)
                .Empty()
                    .WithMessage("The ClosedDateUtc must be empty if the status is not Completed or Canceled."));
    }
}