using Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;

namespace Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Validators;

public sealed class StrategicInitiativeKpiUpsertParametersValidator : CustomValidator<StrategicInitiativeKpiUpsertParameters>
{
    public StrategicInitiativeKpiUpsertParametersValidator()
    { 
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.Description)
            .MaximumLength(512);

        RuleFor(x => x.TargetValue)
            .NotEmpty();

        RuleFor(x => x.Unit)
            .IsInEnum()
            .WithMessage("A valid KPI unit must be selected.");

        RuleFor(x => x.TargetDirection)
            .IsInEnum()
            .WithMessage("A valid KPI direction must be selected.");
    }
}
