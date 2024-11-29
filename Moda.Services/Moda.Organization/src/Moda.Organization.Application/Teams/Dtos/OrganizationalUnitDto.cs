namespace Moda.Organization.Application.Teams.Dtos;
public sealed record OrganizationalUnitDto
{
    public Guid Id { get; set; }
    public int Key { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required string Type { get; set; }
    public List<OrganizationalUnitDto>? Children { get; set; }
}
