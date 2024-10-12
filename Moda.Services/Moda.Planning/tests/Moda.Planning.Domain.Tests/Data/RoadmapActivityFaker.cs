using Moda.Common.Domain.Enums;
using Moda.Common.Models;
using Moda.Planning.Domain.Models.Roadmaps;
using Moda.Tests.Shared.Data;

namespace Moda.Planning.Domain.Tests.Data;
public class RoadmapActivityFaker : PrivateConstructorFaker<RoadmapActivity>
{
    public RoadmapActivityFaker(Guid? roadmapId = null, LocalDate? localDate = null)
    {
        RoadmapId = roadmapId;
        BaseDate = localDate ?? LocalDate.FromDateTime(DateTime.Today);

        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Name, f => f.Random.Words(3));
        RuleFor(x => x.Description, f => f.Random.Words(5));
        RuleFor(x => x.DateRange, f => new LocalDateRange(BaseDate, BaseDate.PlusDays(10)));
        RuleFor(x => x.Color, f => string.Format("#{0:X6}", f.Random.Hexadecimal(0x1000000)));
        RuleFor(x => x.ParentId, f => null);
        RuleFor(x => x.Order, f => f.Random.Int(1,1000));
    }

    public Guid? RoadmapId { get; }
    public LocalDate BaseDate { get; }
}

public static class RoadmapActivityFakerExtensions
{
    public static RoadmapActivityFaker WithData(this RoadmapActivityFaker faker, Guid? id = null, Guid? roadmapId = null, string? name = null, string? description = null, LocalDateRange? dateRange = null, Visibility? visibility = null, Guid? parentId = null, int? order = null, string? color = null)
    {
        if (id.HasValue) { faker.RuleFor(x => x.Id, id.Value); }
        if (roadmapId.HasValue) { faker.RuleFor(x => x.RoadmapId, roadmapId.Value); }
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (!string.IsNullOrWhiteSpace(description)) { faker.RuleFor(x => x.Description, description); }
        if (dateRange is not null) { faker.RuleFor(x => x.DateRange, dateRange); }
        if (parentId.HasValue) { faker.RuleFor(x => x.ParentId, parentId); }
        if (order.HasValue) { faker.RuleFor(x => x.Order, order); }
        if (!string.IsNullOrWhiteSpace(color)) { faker.RuleFor(x => x.Color, color); }

        return faker;
    }

    //public static RoadmapActivity WithChildren(this RoadmapActivityFaker faker, int childrenCount)
    //{
    //    var parentId = Guid.NewGuid();

    //    var childFaker = new RoadmapFaker(faker.BaseDate);

    //    List<Roadmap> children = new(childrenCount);
    //    for (int i = 0; i < childrenCount; i++)
    //    {
    //        var child = childFaker.WithData(parentId: parentId, order: i + 1).Generate();
    //        children.Add(child);
    //    }

        
    //    faker.RuleFor("_children", f => children.ToList());

    //    return faker.WithData(id: parentId).Generate();
    //}
}
