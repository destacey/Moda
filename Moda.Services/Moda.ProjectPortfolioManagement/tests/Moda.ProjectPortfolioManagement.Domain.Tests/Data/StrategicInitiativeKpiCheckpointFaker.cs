using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.Tests.Shared;
using Moda.Tests.Shared.Data;
using NodaTime;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Data;

public sealed class StrategicInitiativeKpiCheckpointFaker : PrivateConstructorFaker<StrategicInitiativeKpiCheckpoint>
{
    public StrategicInitiativeKpiCheckpointFaker(TestingDateTimeProvider dateTimeProvider)
    {
        var pastDate = dateTimeProvider.Now.Minus(Duration.FromDays(FakerHub.Random.Int(1, 200)));

        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.KpiId, f => f.Random.Guid());
        RuleFor(x => x.TargetValue, f => f.Random.Double(0, 100));
        RuleFor(x => x.CheckpointDate, f => pastDate);
    }
}

public static class StrategicInitiativeKpiCheckpointFakerExtensions
{
    public static StrategicInitiativeKpiCheckpointFaker WithData(
        this StrategicInitiativeKpiCheckpointFaker faker,
        Guid? id = null,
        Guid? kpiId = null,
        double? targetValue = null,
        Instant? checkpointDate = null)
    {
        if (id.HasValue) { faker.RuleFor(x => x.Id, id.Value); }
        if (kpiId.HasValue) { faker.RuleFor(x => x.KpiId, kpiId.Value); }
        if (targetValue.HasValue) { faker.RuleFor(x => x.TargetValue, targetValue.Value); }
        if (checkpointDate.HasValue) { faker.RuleFor(x => x.CheckpointDate, checkpointDate.Value); }

        return faker;
    }
}
