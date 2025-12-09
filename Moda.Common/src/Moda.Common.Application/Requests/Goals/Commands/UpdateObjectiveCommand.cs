using Moda.Common.Domain.Enums.Goals;

namespace Moda.Common.Application.Requests.Goals.Commands;

public sealed record UpdateObjectiveCommand(Guid Id, string Name, string? Description, ObjectiveStatus Status, double Progress, Guid? OwnerId, LocalDate? StartDate, LocalDate? TargetDate) : ICommand<Guid>;

public sealed class UpdateObjectiveCommandValidator : CustomValidator<UpdateObjectiveCommand>
{
    public UpdateObjectiveCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(o => o.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(o => o.Description)
            .MaximumLength(1024);

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

        When(o => o.StartDate.HasValue && o.TargetDate.HasValue, () =>
        {
            RuleFor(o => o.StartDate)
                .LessThan(o => o.TargetDate)
                .WithMessage("The start date must be before the target date.");
        });
    }
}