using Wayd.Planning.Domain.Models.Roadmaps;
using Wayd.Tests.Shared.Data;

namespace Wayd.Planning.Domain.Tests.Data;

public class RoadmapManagerFaker : PrivateConstructorFaker<RoadmapManager>
{
    public RoadmapManagerFaker(Guid roadmapId)
    {
        RuleFor(x => x.RoadmapId, roadmapId);
        RuleFor(x => x.ManagerId, f => f.Random.Guid());
    }
}

public static class RoadmapManagerFakerExtensions
{
    public static RoadmapManagerFaker WithData(this RoadmapManagerFaker faker, Guid? roadmapId = null, Guid? managerId = null)
    {
        if (roadmapId.HasValue) { faker.RuleFor(x => x.RoadmapId, roadmapId.Value); }
        if (managerId.HasValue) { faker.RuleFor(x => x.ManagerId, managerId.Value); }

        return faker;
    }
}
