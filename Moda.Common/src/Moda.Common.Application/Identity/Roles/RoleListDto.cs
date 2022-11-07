namespace Moda.Common.Application.Identity.Roles;

public sealed record RoleListDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}