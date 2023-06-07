using Moda.Goals.Domain.Enums;

namespace Moda.Planning.Application.ProgramIncrements.Dtos;
public sealed record ImportProgramIncrementObjectiveDto(
    int ImportId,
    Guid ProgramIncrementId, 
    Guid TeamId, 
    string Name, 
    string? Description, 
    ObjectiveStatus Status,
    LocalDate? StartDate,
    LocalDate? TargetDate, 
    bool IsStretch,
    Instant? ClosedDateUtc);

public sealed class ImportProgramIncrementObjectiveDtoValidator : CustomValidator<ImportProgramIncrementObjectiveDto>
{
    public ImportProgramIncrementObjectiveDtoValidator()
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

        When(o => o.StartDate.HasValue && o.TargetDate.HasValue, () =>
        {
            RuleFor(o => o.StartDate)
                .LessThan(o => o.TargetDate)
                .WithMessage("The start date must be before the target date.");
        });

        When(o => o.Status is ObjectiveStatus.Completed or ObjectiveStatus.Canceled or ObjectiveStatus.Incomplete,
            () => RuleFor(o => o.ClosedDateUtc)
                .NotEmpty()
                    .WithMessage("The ClosedDateUtc can not be empty if the status is Completed, Canceled, or Incomplete."))
            .Otherwise(() => RuleFor(o => o.ClosedDateUtc)
                .Empty()
                    .WithMessage("The ClosedDateUtc must be empty if the status is not Completed, Canceled, or Incomplete."));
    }
}