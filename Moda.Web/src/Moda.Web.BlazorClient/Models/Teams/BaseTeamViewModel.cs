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

    public BaseTeamViewModel(ProgramIncrementTeamReponse dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        Type = dto.Type;
    }

    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;


    // this is needed for the team MudSelect components
    public override bool Equals(object? o)
    {
        var other = o as BaseTeamViewModel;
        return other?.Id == Id;
    }
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString()
    {
        return Name;
    }
}
