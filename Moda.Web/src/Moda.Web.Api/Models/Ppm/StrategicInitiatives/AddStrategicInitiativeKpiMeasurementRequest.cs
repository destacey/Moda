using Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Commands.Kpis;

namespace Moda.Web.Api.Models.Ppm.StrategicInitiatives;

public sealed record AddStrategicInitiativeKpiMeasurementRequest
{
    /// <summary>
    /// The ID of the strategic initiative to which this KPI belongs.
    /// </summary>
    public Guid StrategicInitiativeId { get; set; }

    /// <summary>
    /// The ID of the KPI.
    /// </summary>
    public Guid KpiId { get; set; }

    /// <summary>
    /// The actual measured value for the KPI at this check-in.
    /// </summary>
    public double ActualValue { get; set; }

    /// <summary>
    /// The date and time (in UTC) when the measurement was taken.
    /// </summary>
    public Instant MeasurementDate { get; set; }

    /// <summary>
    /// Optional note providing context for the measurement.
    /// </summary>
    public string? Note { get; set; }

    public AddStrategicInitiativeKpiMeasurementCommand ToAddStrategicInitiativeKpiMeasurementCommand()
    {
        return new AddStrategicInitiativeKpiMeasurementCommand(StrategicInitiativeId, KpiId, ActualValue, MeasurementDate, Note);
    }
}

public sealed class AddStrategicInitiativeKpiMeasurementRequestValidator : AbstractValidator<AddStrategicInitiativeKpiMeasurementRequest>
{
    public AddStrategicInitiativeKpiMeasurementRequestValidator()
    {
        RuleFor(x => x.StrategicInitiativeId)
            .NotEmpty();

        RuleFor(x => x.KpiId)
            .NotEmpty();

        RuleFor(x => x.ActualValue)
            .NotEmpty();

        RuleFor(x => x.MeasurementDate)
            .NotEmpty();

        RuleFor(x => x.Note)
            .MaximumLength(1024);
    }
}
