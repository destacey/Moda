namespace Moda.Web.BlazorClient;

public static class StringHelpers
{

    public static string GetTeamDetailsUrl(string teamType, int localId)
    {
        if (string.IsNullOrWhiteSpace(teamType))
            return string.Empty;

        return teamType == "Team"
            ? $"teams/{localId}"
            : $"teams-of-teams/{localId}";
    }
}
