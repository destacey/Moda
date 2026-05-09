using Mapster;
using Wayd.Common.Application.Dtos;
using Wayd.Common.Domain.Enums.Organization;
using Wayd.Organization.Application.Models;

namespace Wayd.Organization.Application.Teams.Dtos;

internal record TeamMemberFlatDto : IMapFrom<TeamMember>
{
    public required TeamMemberEmployeeDto Employee { get; set; }
    public required TeamNavigationDto Team { get; set; }
    public required NavigationDto Role { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<TeamMember, TeamMemberFlatDto>()
            .Map(dest => dest.Employee, src => new TeamMemberEmployeeDto
            {
                Id = src.Employee.Id,
                Key = src.Employee.Key,
                Name = src.Employee.Name.FirstName + " " + src.Employee.Name.LastName,
                Email = src.Employee.Email.Value,
                JobTitle = src.Employee.JobTitle,
            })
            .Map(dest => dest.Team, src => new TeamNavigationDto
            {
                Id = src.Team.Id,
                Key = src.Team.Key,
                Name = src.Team.Name,
                Type = src.Team.Type == TeamType.TeamOfTeams ? "Team of Teams" : "Team",
            })
            .Map(dest => dest.Role, src => new NavigationDto
            {
                Id = src.Role.Id,
                Key = src.Role.Key,
                Name = src.Role.Name,
            });
    }
}
