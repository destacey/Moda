using Microsoft.AspNetCore.Identity;
using Wayd.Common.Domain.Employees;
using NodaTime;

namespace Wayd.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public Instant? LastActivityAt { get; set; }

    public string LoginProvider { get; set; } = null!;

    /// <summary>
    /// Staged tenant migration target. When set, an Entra login from this tenant
    /// triggers a transactional rebind of the user's active <see cref="UserIdentity"/>
    /// row in the exchange handler. Cleared on completion or admin cancellation.
    /// </summary>
    public string? PendingMigrationTenantId { get; set; }

    public bool MustChangePassword { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public UserPreferences Preferences { get; set; } = new();

    public ICollection<ApplicationUserRole> UserRoles { get; set; } = [];

    public ICollection<UserIdentity> Identities { get; set; } = [];
}