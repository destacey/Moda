using Moda.Common.Application.Dtos;
using Moda.Common.Application.Identity.Roles;

namespace Moda.Common.Application.Identity.Users;

public sealed record UserDetailsDto
{
    public string Id { get; set; } = null!;

    public string? UserName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;

    public string? PhoneNumber { get; set; }

    public Instant? LastActivityAt { get; set; }

    public NavigationDto? Employee { get; set; }

    public List<RoleListDto> Roles { get; set; } = [];
}

