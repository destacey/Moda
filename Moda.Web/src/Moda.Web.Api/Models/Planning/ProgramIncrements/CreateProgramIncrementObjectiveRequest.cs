using Moda.Planning.Application.ProgramIncrements.Commands;

namespace Moda.Web.Api.Models.Planning.ProgramIncrements;

public class CreateProgramIncrementObjectiveRequest
{
    public Guid ProgramIncrementId { get; set; }
    public Guid TeamId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public LocalDate? StartDate { get; set; }
    public LocalDate? TargetDate { get; set; }
    public bool IsStretch { get; set; }

    public CreateProgramIncrementObjectiveCommand ToCreateProgramIncrementObjectiveCommand()
    {
        return new CreateProgramIncrementObjectiveCommand(ProgramIncrementId, TeamId, Name, Description, StartDate, TargetDate, IsStretch);
    }
}

public sealed class CreateProgramIncrementObjectiveRequestValidator : CustomValidator<CreateProgramIncrementObjectiveRequest>
{
    public CreateProgramIncrementObjectiveRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.TeamId)
            .NotEmpty()
            .WithMessage("A plan must be selected.");

        RuleFor(r => r.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(r => r.Description)
            .MaximumLength(1024);

        When(o => o.StartDate.HasValue && o.TargetDate.HasValue, () =>
        {
            RuleFor(r => r.StartDate)
                .LessThan(r => r.TargetDate)
                .WithMessage("The start date must be before the target date.");
        });
    }
}
