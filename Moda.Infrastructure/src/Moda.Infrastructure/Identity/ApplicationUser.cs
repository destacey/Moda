using Microsoft.AspNetCore.Identity;
using NodaTime;

namespace Moda.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public string? RefreshToken { get; set; }
    public Instant RefreshTokenExpiryTime { get; set; }

    public string? ObjectId { get; set; }
}