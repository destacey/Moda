using Moda.Web.BlazorClient.Infrastructure.ApiClient;

namespace Moda.Web.BlazorClient.Models.Teams;

public record TeamListViewModel
{
    public System.Guid Id { get; set; } = default!;
    public int LocalId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Type { get; set; } = default!;
    public bool IsActive { get; set; } = default!;
    public NavigationDto? TeamOfTeams { get; set; }

    // hack until the datagrid nullable issue is fixed
    public string? TeamOfTeamsName { get; set; }

    public string DetailsUrl
        => StringHelpers.GetTeamDetailsUrl(Type, LocalId);

    public string? TeamOfTeamsDetailsUrl
        => TeamOfTeams is not null ? $"teams-of-teams/{TeamOfTeams.LocalId}" : null;

    public static TeamListViewModel FromTeamListDto(TeamListDto dto)
    {
        return new TeamListViewModel()
        {
            Id = dto.Id,
            LocalId = dto.LocalId,
            Name = dto.Name,
            Code = dto.Code,
            Type = dto.Type,
            IsActive = dto.IsActive,
            TeamOfTeams = dto.TeamOfTeams,
            TeamOfTeamsName = dto.TeamOfTeams?.Name
        };
    }

    public static TeamListViewModel FromTeamOfTeamsListDto(TeamOfTeamsListDto dto)
    {
        return new TeamListViewModel()
        {
            Id = dto.Id,
            LocalId = dto.LocalId,
            Name = dto.Name,
            Code = dto.Code,
            Type = dto.Type,
            IsActive = dto.IsActive,
            TeamOfTeams = dto.TeamOfTeams,
            TeamOfTeamsName = dto.TeamOfTeams?.Name
        };
    }

    public static TeamListViewModel FromProgramIncrementTeamResponse(ProgramIncrementTeamReponse dto)
    {
        return new TeamListViewModel()
        {
            Id = dto.Id,
            LocalId = dto.LocalId,
            Name = dto.Name,
            Code = dto.Code,
            Type = dto.Type,
            IsActive = dto.IsActive,
            TeamOfTeams = dto.TeamOfTeams,
            TeamOfTeamsName = dto.TeamOfTeams?.Name
        };
    }
}
