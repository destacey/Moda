namespace Moda.Common.Application.Identity.Users;

public sealed record ToggleUserStatusRequest
{
    public bool ActivateUser { get; set; }
    public string? UserId { get; set; }
}
