namespace Moda.Web.Api.Models.UserManagement.Users;

public sealed record ToggleUserStatusRequest
{
    public string UserId { get; set; } = null!;
    public bool ActivateUser { get; set; }

    public ToggleUserStatusCommand ToToggleUserStatusCommand()
        => new(UserId, ActivateUser);
}
