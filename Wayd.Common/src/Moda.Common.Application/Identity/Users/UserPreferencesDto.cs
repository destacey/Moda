namespace Wayd.Common.Application.Identity.Users;

public sealed record UserPreferencesDto
{
    public Dictionary<string, bool> Tours { get; init; } = [];
}
