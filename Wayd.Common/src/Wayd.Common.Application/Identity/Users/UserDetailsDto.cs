using Wayd.Common.Application.Dtos;
using Wayd.Common.Application.Identity.Roles;

namespace Wayd.Common.Application.Identity.Users;

public sealed record UserDetailsDto
{
    public string Id { get; set; } = null!;

    public string? UserName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;

    public Instant? LockoutEnd { get; set; }

    public string? PhoneNumber { get; set; }

    public string LoginProvider { get; set; } = null!;

    public Instant? LastActivityAt { get; set; }

    public NavigationDto? Employee { get; set; }

    public List<RoleListDto> Roles { get; set; } = [];
}

