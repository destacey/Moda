namespace Moda.Core.Application.Identity.Users;

public sealed record ToggleUserStatusRequest
{
    public bool ActivateUser { get; set; }
    public string? UserId { get; set; }
}
