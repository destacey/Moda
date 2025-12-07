using Moda.Common.Domain.Enums.Goals;

namespace Moda.Common.Application.Requests.Goals.Commands;

public sealed record ImportObjectiveCommand(string Name, string? Description, ObjectiveType Type, ObjectiveStatus Status, double Progress, Guid? OwnerId, Guid? PlanId, LocalDate? StartDate, LocalDate? TargetDate, Instant? ClosedDate, int? Order) : ICommand<Guid>;

public sealed class ImportObjectiveCommandValidator : CustomValidator<ImportObjectiveCommand>
{
    public ImportObjectiveCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(o => o.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(o => o.Description)
            .MaximumLength(1024);

        RuleFor(o => o.Type)
            .IsInEnum()
            .WithMessage("A valid objective type must be selected.");

        RuleFor(o => o.Status)
            .IsInEnum()
            .WithMessage("A valid objective status must be selected.");

        RuleFor(o => o.Progress)
            .InclusiveBetween(0.0d, 100.0d)
            .WithMessage("The progress must be between 0 and 100.");

        When(o => o.OwnerId.HasValue, () =>
        {
            RuleFor(o => o.OwnerId)
                .NotEmpty()
                .WithMessage("An owner must be selected.");
        });

        When(o => o.PlanId.HasValue, () =>
        {
            RuleFor(o => o.PlanId)
                .NotEmpty()
                .WithMessage("A plan must be selected.");
        });

        When(o => o.StartDate.HasValue && o.TargetDate.HasValue, () =>
        {
            RuleFor(o => o.StartDate)
                .LessThan(o => o.TargetDate)
                .WithMessage("The start date must be before the target date.");
        });

        When(o => o.Status is ObjectiveStatus.Completed or ObjectiveStatus.Canceled or ObjectiveStatus.Missed,
            () => RuleFor(o => o.ClosedDate)
                .NotEmpty()
                    .WithMessage("The ClosedDateUtc can not be empty if the status is Completed or Canceled."))
            .Otherwise(() => RuleFor(o => o.ClosedDate)
                .Empty()
                    .WithMessage("The ClosedDateUtc must be empty if the status is not Completed or Canceled"));
    }
}