using Moda.Common.Application.Dtos;

namespace Moda.Organization.Application.Teams.Dtos;

/// <summary>
/// Represents an organizational unit.
/// </summary>
public sealed record OrganizationalUnitDto
{
    public Guid Id { get; init; }
    public int Key { get; init; }
    public required string Name { get; init; }
    public required string Code { get; init; }
    public required SimpleNavigationDto Type { get; init; }
    public int Level { get; init; }
    public required string Path { get; init; }
    public List<OrganizationalUnitDto>? Children { get; init; }
}
