using Moda.Common.Domain.Enums;
using Moda.Planning.Domain.Models;

namespace Moda.Planning.Domain.Tests.Data;
public class SimpleHealthCheckFaker : Faker<SimpleHealthCheck>
{
    public SimpleHealthCheckFaker(Instant timestamp, Guid? objectId = null)
    {
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.ObjectId, f => objectId ?? f.Random.Guid());
        RuleFor(x => x.Status, f => f.PickRandom<HealthStatus>());
        RuleFor(x => x.ReportedOn, timestamp);
        RuleFor(x => x.Expiration, timestamp.Plus(Duration.FromDays(5)));
    }
}

public static class SimpleHealthCheckFakerExtensions
{
    public static List<SimpleHealthCheck> MultipleWithSameObjectId(this SimpleHealthCheckFaker faker, Guid objectId, int count)
    {

        var healthChecks = new List<SimpleHealthCheck>()
        {
            faker.UsePrivateConstructor().Generate()
        };

        for (int i = 1; i < count; i++)
        {
            var previous = healthChecks.Last();
            var healthCheckFaker = new SimpleHealthCheckFaker(previous.Expiration, objectId).UsePrivateConstructor().Generate();
            healthChecks.Add(healthCheckFaker);
        }

        return healthChecks;
    }
}
