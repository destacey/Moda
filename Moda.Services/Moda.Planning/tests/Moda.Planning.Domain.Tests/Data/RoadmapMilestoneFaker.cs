using Moda.Planning.Domain.Models.Roadmaps;
using Moda.Tests.Shared.Data;

namespace Moda.Planning.Domain.Tests.Data;
public class RoadmapMilestoneFaker : PrivateConstructorFaker<RoadmapMilestone>
{
    public RoadmapMilestoneFaker(Guid? roadmapId = null, LocalDate? localDate = null)
    {
        BaseDate = localDate ?? LocalDate.FromDateTime(DateTime.Today);

        RuleFor(x => x.RoadmapId, f => roadmapId ?? f.Random.Guid());
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Name, f => f.Random.Words(3));
        RuleFor(x => x.Description, f => f.Random.Words(5));
        RuleFor(x => x.Date, f => BaseDate);
        RuleFor(x => x.Color, f => string.Format("#{0:X6}", f.Random.Hexadecimal(0x1000000)));
        RuleFor(x => x.ParentId, f => null);
    }

    public LocalDate BaseDate { get; }
}

public static class RoadmapMilestoneFakerExtensions
{
    public static RoadmapMilestoneFaker WithData(this RoadmapMilestoneFaker faker, Guid? id = null, Guid? roadmapId = null, string? name = null, string? description = null, LocalDate? date = null, Guid? parentId = null, RoadmapActivity? parent = null, string? color = null)
    {
        if (id.HasValue) { faker.RuleFor(x => x.Id, id.Value); }
        if (roadmapId.HasValue) { faker.RuleFor(x => x.RoadmapId, roadmapId.Value); }
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (!string.IsNullOrWhiteSpace(description)) { faker.RuleFor(x => x.Description, description); }
        if (date is not null) { faker.RuleFor(x => x.Date, date); }
        if (parentId.HasValue && parent is null) { faker.RuleFor(x => x.ParentId, parentId); }
        if (parent is not null) {
            faker.RuleFor(x => x.ParentId, parent.Id);
            faker.RuleFor(x => x.Parent, parent);
        }
        if (!string.IsNullOrWhiteSpace(color)) { faker.RuleFor(x => x.Color, color); }

        return faker;
    }
}
