using Microsoft.AspNetCore.Identity;

namespace Moda.Infrastructure.Identity;

public class ApplicationUserRole : IdentityUserRole<string>
{
    public ApplicationRole Role { get; set; } = null!;
}
