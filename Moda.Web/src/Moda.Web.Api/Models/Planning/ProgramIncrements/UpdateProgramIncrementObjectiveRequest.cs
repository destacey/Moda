using Moda.Planning.Application.ProgramIncrements.Commands;
using Moda.Planning.Domain.Enums;

namespace Moda.Web.Api.Models.Planning.ProgramIncrements;

public class UpdateProgramIncrementObjectiveRequest
{
    public Guid ProgramIncrementId { get; set; }
    public Guid ObjectiveId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int StatusId { get; set; }
    public double Progress { get; set; }
    public LocalDate? StartDate { get; set; }
    public LocalDate? TargetDate { get; set; }
    public bool IsStretch { get; set; }

    public UpdateProgramIncrementObjectiveCommand ToUpdateProgramIncrementObjectiveCommand()
    {
        return new UpdateProgramIncrementObjectiveCommand(ProgramIncrementId, ObjectiveId, Name, Description, (ObjectiveStatus)StatusId, Progress, StartDate, TargetDate, IsStretch);
    }
}

public sealed class UpdateProgramIncrementObjectiveRequestValidator : CustomValidator<UpdateProgramIncrementObjectiveRequest>
{
    public UpdateProgramIncrementObjectiveRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(o => o.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(o => o.Description)
            .MaximumLength(1024);

        RuleFor(o => (ObjectiveStatus)o.StatusId)
            .IsInEnum()
            .WithMessage("A valid objective status must be selected.");

        RuleFor(o => o.Progress)
            .InclusiveBetween(0.0d, 100.0d)
            .WithMessage("The progress must be between 0 and 100.");

        When(o => o.StartDate.HasValue && o.TargetDate.HasValue, () =>
        {
            RuleFor(r => r.StartDate)
                .LessThan(r => r.TargetDate)
                .WithMessage("The start date must be before the target date.");
        });
    }
}
