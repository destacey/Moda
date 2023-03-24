using Moda.Common.Application.Interfaces;
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

    public string DetailsUrl
        => Type == "Team" 
            ? $"teams/{LocalId}"
            : $"teams-of-teams/{LocalId}";

    public static TeamListViewModel FromTeamListDto(TeamListDto dto)
    {
        return new TeamListViewModel()
        {
            Id = dto.Id,
            LocalId = dto.LocalId,
            Name = dto.Name,
            Code = dto.Code,
            Type = dto.Type,
            IsActive = dto.IsActive
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
            IsActive = dto.IsActive
        };
    }
}
