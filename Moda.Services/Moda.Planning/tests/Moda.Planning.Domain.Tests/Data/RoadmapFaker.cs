using Moda.Common.Domain.Enums;
using Moda.Common.Models;
using Moda.Planning.Domain.Models;
using Moda.Tests.Shared.Data;

namespace Moda.Planning.Domain.Tests.Data;
public class RoadmapFaker : PrivateConstructorFaker<Roadmap>
{
    public RoadmapFaker(LocalDate? localDate = null)
    {
        BaseDate = localDate ?? LocalDate.FromDateTime(DateTime.Today);

        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Key, f => f.Random.Int(1000, 10000));
        RuleFor(x => x.Name, f => f.Random.Words(3));
        RuleFor(x => x.Description, f => f.Random.Words(5));
        RuleFor(x => x.DateRange, f => new LocalDateRange(BaseDate, BaseDate.PlusDays(10)));
        RuleFor(x => x.Visibility, f => f.PickRandom<Visibility>());
        //RuleFor(x => x.Managers, f => managerFaker.Generate(1)); // TODO not working
        RuleFor(x => x.ParentId, f => null);
        RuleFor(x => x.Order, f => null);
    }

    public LocalDate BaseDate { get; }
}

public static class RoadmapFakerExtensions
{
    public static RoadmapFaker WithData(this RoadmapFaker faker, Guid? id = null, string? name = null, string? description = null, LocalDateRange? dateRange = null, Visibility? visibility = null, Guid? parentId = null, int? order = null)
    {
        if (id.HasValue) { faker.RuleFor(x => x.Id, id.Value); }
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (!string.IsNullOrWhiteSpace(description)) { faker.RuleFor(x => x.Description, description); }
        if (dateRange is not null) { faker.RuleFor(x => x.DateRange, dateRange); }
        if (visibility.HasValue) { faker.RuleFor(x => x.Visibility, visibility); }
        // TODO - Add managers
        if (parentId.HasValue) { faker.RuleFor(x => x.ParentId, parentId); }
        if (order.HasValue) { faker.RuleFor(x => x.Order, order); }

        return faker;
    }

    public static Roadmap WithChildren(this RoadmapFaker faker, int childrenCount)
    {
        var roadmapId = Guid.NewGuid();

        var childFaker = new RoadmapFaker(faker.BaseDate);

        List<Roadmap> children = new(childrenCount);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = childFaker.WithData(parentId: roadmapId, order: i + 1).Generate();
            children.Add(child);
        }

        var managers = new RoadmapManagerFaker(roadmapId).Generate(1);

        faker.RuleFor("_children", f => children.ToList());
        faker.RuleFor("_managers", f => managers.ToList());

        return faker.WithData(id: roadmapId).Generate();
    }
}
