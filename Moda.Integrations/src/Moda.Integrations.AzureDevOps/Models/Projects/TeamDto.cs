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
    public static AzdoTeam ToAzdoTeam(this TeamDto team, Guid workspaceId, Guid? boardId)
    {
        return new AzdoTeam
        {
            Id = team.Id,
            Name = team.Name,
            WorkspaceId = workspaceId,
            BoardId = boardId
        };
    }
}
