using Moda.Common.Domain.Models.KeyPerformanceIndicators;
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
            .MaximumLength(512)
            .When(x => x.Description is not null);

        RuleFor(x => x.StartingValue)
            .LessThan(x => x.TargetValue)
            .When(x => x.StartingValue.HasValue && x.TargetDirection == KpiTargetDirection.Increase)
            .WithMessage("Starting value must be less than the target value when the target direction is Increase.");

        RuleFor(x => x.StartingValue)
            .GreaterThan(x => x.TargetValue)
            .When(x => x.StartingValue.HasValue && x.TargetDirection == KpiTargetDirection.Decrease)
            .WithMessage("Starting value must be greater than the target value when the target direction is Decrease.");

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
