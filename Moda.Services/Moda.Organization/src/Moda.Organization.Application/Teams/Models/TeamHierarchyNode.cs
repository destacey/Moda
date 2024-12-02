using Moda.Common.Domain.Enums.Organization;

namespace Moda.Organization.Application.Teams.Models;
public sealed record TeamHierarchyNode
{
    public Guid Id { get; set; }
    public int Key { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public TeamType Type { get; set; }
    public Guid? ParentId { get; set; }
    public int Level { get; set; }
    public required string Path { get; set; }
}
