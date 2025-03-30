using Moda.Common.Domain.Models.KeyPerformanceIndicators;
using Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Commands.Kpis;
using Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;

namespace Moda.Web.Api.Models.Ppm.StrategicInitiatives;

public sealed record CreateStrategicInitiativeKpiRequest
{
    /// <summary>
    /// The ID of the strategic initiative to which this KPI belongs.
    /// </summary>
    public Guid StrategicInitiativeId { get; set; }

    /// <summary>
    /// The name of the KPI.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// A description of what the KPI measures.
    /// </summary>
    public string Description { get; set; } = default!;

    /// <summary>
    /// The target value that defines success for the KPI.
    /// </summary>
    public double TargetValue { get; set; }

    /// <summary>
    /// The ID of the unit of measurement for the KPI.
    /// </summary>
    public int Unit { get; set; }

    /// <summary>
    /// The ID of the target direction for the KPI.
    /// </summary>
    public int TargetDirection { get; set; }

    public CreateStrategicInitiativeKpiCommand ToCreateStrategicInitiativeKpiCommand()
    {
        var parameters = new StrategicInitiativeKpiUpsertParameters(Name, Description, TargetValue, (KpiUnit)Unit, (KpiTargetDirection)TargetDirection);

        return new CreateStrategicInitiativeKpiCommand(StrategicInitiativeId, parameters);
    }
}

public sealed class CreateStrategicInitiativeKpiRequestValidator : AbstractValidator<CreateStrategicInitiativeKpiRequest>
{
    public CreateStrategicInitiativeKpiRequestValidator()
    {
        RuleFor(x => x.StrategicInitiativeId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(512);

        RuleFor(x => x.TargetValue)
            .NotEmpty();

        RuleFor(x => (KpiUnit)x.Unit)
            .IsInEnum()
            .WithMessage("A valid KPI unit must be selected.");

        RuleFor(x => (KpiTargetDirection)x.TargetDirection)
            .IsInEnum()
            .WithMessage("A valid KPI direction must be selected.");
    }
}
