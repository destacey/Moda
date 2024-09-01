using Moda.Common.Domain.Employees;

namespace Moda.Planning.Domain.Models;
public sealed class RoadmapManagers
{
    private RoadmapManagers() { }

    internal RoadmapManagers(Roadmap roadmap, Guid managerId)
    {
        RoadmapId = roadmap.Id;
        ManagerId = managerId;
    }

    public Guid RoadmapId { get; private init; }

    public Guid ManagerId { get; private init; }

    public Employee? Manager { get; private init; }
}
