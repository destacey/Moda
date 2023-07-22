using Moda.Web.BlazorClient.Infrastructure.ApiClient;

namespace Moda.Web.BlazorClient.Models.Teams;

public class BaseTeamViewModel
{
    // TODO move these to an interface

    public BaseTeamViewModel() { }

    public BaseTeamViewModel(TeamDetailsDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        Type = dto.Type;
    }

    public BaseTeamViewModel(TeamListDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        Type = dto.Type;
    }

    public BaseTeamViewModel(TeamOfTeamsDetailsDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        Type = dto.Type;
    }

    public BaseTeamViewModel(TeamOfTeamsListDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        Type = dto.Type;
    }

    public BaseTeamViewModel(ProgramIncrementTeamResponse dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        Type = dto.Type;
    }

    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Type { get; set; } = default!;
}
