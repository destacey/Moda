using Moda.Common.Domain.Enums.Goals;

namespace Moda.Common.Application.Requests.Goals.Commands;

public sealed record CreateObjectiveCommand(string Name, string? Description, ObjectiveType Type, Guid? OwnerId, Guid? PlanId, LocalDate? StartDate, LocalDate? TargetDate, int? Order) : ICommand<Guid>;

public sealed class CreateObjectiveCommandValidator : CustomValidator<CreateObjectiveCommand>
{
    public CreateObjectiveCommandValidator()
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
    }
}
