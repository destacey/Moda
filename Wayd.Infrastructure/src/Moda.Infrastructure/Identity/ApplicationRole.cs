using Microsoft.AspNetCore.Identity;

namespace Wayd.Infrastructure.Identity;

public class ApplicationRole : IdentityRole
{
    public ApplicationRole(string name, string? description = null)
        : base(name)
    {
        Description = description?.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
    }

    public string? Description { get; set; }

    public ICollection<ApplicationUserRole> UserRoles { get; set; } = [];

    public void Update(string name, string? description = null)
    {
        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
        Description = description?.Trim();
    }
}