using Moda.Planning.Application.PlanningIntervals.Commands;

namespace Moda.Web.Api.Models.Planning.PlanningIntervals;

public sealed record UpdatePlanningIntervalRequest
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets the team name.</summary>
    /// <value>The team name.</value>
    public required string Name { get; set; }

    /// <summary>Gets the team description.</summary>
    /// <value>The team description.</value>
    public string? Description { get; set; }

    /// <summary>Gets or sets the objectives locked.</summary>
    /// <value><c>true</c> if [objectives locked]; otherwise, <c>false</c>.</value>
    public bool ObjectivesLocked { get; set; }

    public UpdatePlanningIntervalCommand ToUpdatePlanningIntervalCommand()
    {
        return new UpdatePlanningIntervalCommand(Id, Name, Description, ObjectivesLocked);
    }
}

public sealed class UpdatePlanningIntervalRequestValidator : CustomValidator<UpdatePlanningIntervalRequest>
{
    public UpdatePlanningIntervalRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(t => t.Description)
            .MaximumLength(1024);
    }
}
