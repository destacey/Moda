namespace Moda.Common.Application.Identity.Users;

public sealed record ToggleUserStatusCommand
{
    public ToggleUserStatusCommand(string userId, bool activateUser)
    {
        UserId = userId;
        ActivateUser = activateUser;
    }

    public string UserId { get; }
    public bool ActivateUser { get; }
}
