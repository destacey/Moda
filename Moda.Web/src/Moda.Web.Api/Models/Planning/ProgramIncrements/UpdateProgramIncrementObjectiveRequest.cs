using Moda.Goals.Domain.Enums;
using Moda.Planning.Application.ProgramIncrements.Commands;

namespace Moda.Web.Api.Models.Planning.ProgramIncrements;

public class UpdateProgramIncrementObjectiveRequest
{
    public Guid ProgramIncrementId { get; set; }
    public Guid ObjectiveId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int StatusId { get; set; }
    public LocalDate? StartDate { get; set; }
    public LocalDate? TargetDate { get; set; }
    public bool IsStretch { get; set; }

    public UpdateProgramIncrementObjectiveCommand ToUpdateProgramIncrementObjectiveCommand()
    {
        return new UpdateProgramIncrementObjectiveCommand(ProgramIncrementId, ObjectiveId, Name, Description, (ObjectiveStatus)StatusId, StartDate, TargetDate, IsStretch);
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

        When(o => o.StartDate.HasValue && o.TargetDate.HasValue, () =>
        {
            RuleFor(r => r.StartDate)
                .LessThan(r => r.TargetDate)
                .WithMessage("The start date must be before the target date.");
        });
    }
}
