using Microsoft.AspNetCore.Identity;
using Wayd.Common.Domain.Employees;
using NodaTime;

namespace Wayd.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
    public string? ObjectId { get; set; }
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public Instant? LastActivityAt { get; set; }

    public string LoginProvider { get; set; } = null!;

    public bool MustChangePassword { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public UserPreferences Preferences { get; set; } = new();

    public ICollection<ApplicationUserRole> UserRoles { get; set; } = [];
}