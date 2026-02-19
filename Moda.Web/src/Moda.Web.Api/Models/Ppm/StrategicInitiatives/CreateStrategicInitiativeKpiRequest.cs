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
    public string? Description { get; set; }

    /// <summary>
    /// The target value that defines success for the KPI.
    /// </summary>
    public double TargetValue { get; set; }

    /// <summary>
    /// The unit of measurement for the KPI.
    /// </summary>
    public KpiUnit Unit { get; set; }

    /// <summary>
    /// The target direction for the KPI.
    /// </summary>
    public KpiTargetDirection TargetDirection { get; set; }

    public CreateStrategicInitiativeKpiCommand ToCreateStrategicInitiativeKpiCommand()
    {
        var parameters = new StrategicInitiativeKpiUpsertParameters(Name, Description, TargetValue, Unit, TargetDirection);

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
            .MaximumLength(512)
            .When(x => x.Description is not null);

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
