using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Integrations.AzureDevOps.Models.Contracts;

namespace Moda.Integrations.AzureDevOps.Models.Projects;
internal record TeamDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}

internal static class TeamDtoExtensions
{
    public static AzdoTeam ToAzdoTeam(this TeamDto team, Guid workspaceId)
    {
        return new AzdoTeam
        {
            Id = team.Id,
            Name = team.Name,
            WorkspaceId = workspaceId
        };
    }

    public static List<IExternalTeam> ToIExternalTeams(this List<TeamDto> teams, Guid workspaceId)
    {
        var azdoTeams = new List<IExternalTeam>(teams.Count);
        foreach (var team in teams)
        {
            azdoTeams.Add(team.ToAzdoTeam(workspaceId));
        }
        return azdoTeams;
    }
}
