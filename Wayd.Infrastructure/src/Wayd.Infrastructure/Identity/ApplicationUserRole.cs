using Microsoft.AspNetCore.Identity;

namespace Wayd.Infrastructure.Identity;

public class ApplicationUserRole : IdentityUserRole<string>
{
    public ApplicationRole Role { get; set; } = null!;
}
