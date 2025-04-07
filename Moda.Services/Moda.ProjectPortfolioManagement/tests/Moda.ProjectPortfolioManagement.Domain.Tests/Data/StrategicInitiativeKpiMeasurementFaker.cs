using Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;
using Moda.Tests.Shared;
using Moda.Tests.Shared.Data;
using NodaTime;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Data;

public sealed class StrategicInitiativeKpiMeasurementFaker : PrivateConstructorFaker<StrategicInitiativeKpiMeasurement>
{
    public StrategicInitiativeKpiMeasurementFaker(TestingDateTimeProvider dateTimeProvider)
    {
        var pastDate = dateTimeProvider.Now.Minus(Duration.FromDays(FakerHub.Random.Int(1, 200)));

        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.KpiId, f => f.Random.Guid());
        RuleFor(x => x.ActualValue, f => f.Random.Double(0, 100));
        RuleFor(x => x.MeasurementDate, f => pastDate);
        RuleFor(x => x.MeasuredById, f => f.Random.Guid());
        RuleFor(x => x.Note, f => f.Lorem.Sentence());
    }
}

public static class StrategicInitiativeKpiMeasurementFakerExtensions
{
    public static StrategicInitiativeKpiMeasurementFaker WithData(
        this StrategicInitiativeKpiMeasurementFaker faker,
        Guid? id = null,
        Guid? kpiId = null,
        double? actualValue = null,
        Instant? measurementDate = null,
        Guid? measuredById = null,
        string? note = null)
    {
        if (id.HasValue) { faker.RuleFor(x => x.Id, id.Value); }
        if (kpiId.HasValue) { faker.RuleFor(x => x.KpiId, kpiId.Value); }
        if (actualValue.HasValue) { faker.RuleFor(x => x.ActualValue, actualValue.Value); }
        if (measurementDate.HasValue) { faker.RuleFor(x => x.MeasurementDate, measurementDate.Value); }
        if (measuredById.HasValue) { faker.RuleFor(x => x.MeasuredById, measuredById.Value); }
        if (!string.IsNullOrWhiteSpace(note)) { faker.RuleFor(x => x.Note, note); }

        return faker;
    }
}