using Moda.Common.Domain.Data;

namespace Moda.Common.Domain.Identity;

/// <summary>
/// Read-only domain representation of an application user.
/// Backed by a database view over the Identity.Users table.
/// </summary>
public sealed class User : BaseEntity<string>
{
    private User() { }

    public string UserName { get; private set; } = default!;
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? DisplayName { get; private set; }
    public string? Email { get; private set; }
    public bool IsActive { get; private set; }
}
