namespace Moda.Core.Application.Identity.Users;

public sealed class ToggleUserStatusRequest
{
    public bool ActivateUser { get; set; }
    public string? UserId { get; set; }
}
