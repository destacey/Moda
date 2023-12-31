using Moda.Tests.Shared.Data;

namespace Moda.Health.Tests.Data;
public class HealthCheckFaker : PrivateConstructorFaker<HealthCheck>
{
    public HealthCheckFaker(Instant timestamp, Guid? objectId = null)
    {
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.ObjectId, f => objectId ?? f.Random.Guid());
        RuleFor(x => x.Context, f => f.PickRandom<SystemContext>());
        RuleFor(x => x.Status, f => f.PickRandom<HealthStatus>());
        RuleFor(x => x.ReportedById, f => f.Random.Guid());
        RuleFor(x => x.ReportedOn, timestamp);
        RuleFor(x => x.Expiration, timestamp.Plus(Duration.FromDays(5)));
        RuleFor(x => x.Note, f => f.Random.String2(15));
    }
}

public static class HealthCheckFakerExtensions
{
    public static List<HealthCheck> MultipleWithSameObjectId(this HealthCheckFaker faker, Guid objectId, int count)
    {

        var healthChecks = new List<HealthCheck>()
        {
            faker.Generate()
        };

        for (int i = 1; i < count; i++)
        {
            var previous = healthChecks.Last();
            var healthCheckFaker = new HealthCheckFaker(previous.Expiration, objectId).Generate();
            healthChecks.Add(healthCheckFaker);
        }

        return healthChecks;
    }
}
