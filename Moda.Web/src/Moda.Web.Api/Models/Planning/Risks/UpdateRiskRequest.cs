using Moda.Planning.Application.Risks.Commands;
using Moda.Planning.Domain.Enums;

namespace Moda.Web.Api.Models.Planning.Risks;

public class UpdateRiskRequest
{
    public Guid RiskId { get; set; }
    public Guid TeamId { get; set; }
    public string Summary { get; set; } = default!;
    public string? Description { get; set; }
    public RiskStatus Status { get; set; }
    public RiskCategory Category { get; set; }
    public RiskGrade Impact { get; set; }
    public RiskGrade Likelihood { get; set; }
    public Guid? AssigneeId { get; set; }
    public LocalDate? FollowUpDate { get; set; }
    public string? Response { get; set; }

    public UpdateRiskCommand ToUpdateRiskCommand()
    {
        return new UpdateRiskCommand(RiskId, Summary, Description, TeamId, Status, Category, Impact, Likelihood, AssigneeId, FollowUpDate, Response);
    }
}

public sealed class UpdateRiskCommandValidator : CustomValidator<UpdateRiskCommand>
{
    public UpdateRiskCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(e => e.Summary)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(e => e.Description)
            .MaximumLength(1024);

        RuleFor(e => e.Status)
            .NotNull();

        RuleFor(e => e.Category)
            .NotNull();

        RuleFor(e => e.Impact)
            .NotNull();

        RuleFor(e => e.Likelihood)
            .NotNull();
    }
}
