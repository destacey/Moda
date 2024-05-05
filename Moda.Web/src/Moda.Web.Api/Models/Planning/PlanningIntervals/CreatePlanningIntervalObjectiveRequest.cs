using Moda.Planning.Application.PlanningIntervals.Commands;

namespace Moda.Web.Api.Models.Planning.PlanningIntervals;

public class CreatePlanningIntervalObjectiveRequest
{
    public Guid PlanningIntervalId { get; set; }
    public Guid TeamId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public LocalDate? StartDate { get; set; }
    public LocalDate? TargetDate { get; set; }
    public bool IsStretch { get; set; }
    public int? Order { get; set; }

    public CreatePlanningIntervalObjectiveCommand ToCreatePlanningIntervalObjectiveCommand()
    {
        return new CreatePlanningIntervalObjectiveCommand(PlanningIntervalId, TeamId, Name, Description, StartDate, TargetDate, IsStretch, Order);
    }
}

public sealed class CreatePlanningIntervalObjectiveRequestValidator : CustomValidator<CreatePlanningIntervalObjectiveRequest>
{
    public CreatePlanningIntervalObjectiveRequestValidator()
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

        When(o => o.StartDate.HasValue && o.TargetDate.HasValue, () =>
        {
            RuleFor(o => o.StartDate)
                .LessThan(o => o.TargetDate)
                .WithMessage("The start date must be before the target date.");
        });
    }
}
