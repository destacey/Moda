using Microsoft.AspNetCore.Identity;

namespace Moda.Infrastructure.Identity;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }

    public ApplicationRole(string name, string? description = null)
        : base(name)
    {
        Description = description?.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
    }
}