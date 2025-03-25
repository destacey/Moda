namespace Moda.Web.Api.Models.UserManagement.Users;

public sealed record ToggleUserStatusRequest
{
    public string UserId { get; set; } = default!;
    public bool ActivateUser { get; set; }

    public ToggleUserStatusCommand ToToggleUserStatusCommand()
        => new(UserId, ActivateUser);
}
