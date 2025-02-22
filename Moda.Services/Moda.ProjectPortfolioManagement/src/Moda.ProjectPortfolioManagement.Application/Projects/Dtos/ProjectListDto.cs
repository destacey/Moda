using Moda.Common.Application.Dtos;
using Moda.Common.Application.Employees.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.Projects.Dtos;

public sealed record ProjectListDto : IMapFrom<Project>
{
    public Guid Id { get; set; }

    /// <summary>
    /// The unique key of the project.  This is an alternate key to the Id.
    /// </summary>
    public int Key { get; set; }

    /// <summary>
    /// The name of the project.
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// The current status of the project.
    /// </summary>
    public required SimpleNavigationDto Status { get; set; }

    /// <summary>
    /// The project start date.
    /// </summary>
    public LocalDate? Start { get; set; }

    /// <summary>
    /// The project end date.
    /// </summary>
    public LocalDate? End { get; set; }

    /// <summary>
    /// The portfolio associated with this project.
    /// </summary>
    public required NavigationDto Portfolio { get; set; }

    /// <summary>
    /// The sponsors of the project.
    /// </summary>
    public required List<EmployeeNavigationDto> ProjectSponsors { get; set; } = [];

    /// <summary>
    /// The owners of the project.
    /// </summary>
    public required List<EmployeeNavigationDto> ProjectOwners { get; set; } = [];

    /// <summary>
    /// The managers of the project.
    /// </summary>
    public required List<EmployeeNavigationDto> ProjectManagers { get; set; } = [];

    /// <summary>
    /// The strategic themes associated with this project.
    /// </summary>
    public required List<NavigationDto> StrategicThemes { get; set; } = [];

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<Project, ProjectListDto>()
            .Map(dest => dest.Status, src => SimpleNavigationDto.FromEnum(src.Status))
            .Map(dest => dest.Start, src => src.DateRange != null ? src.DateRange.Start : (LocalDate?)null)
            .Map(dest => dest.End, src => src.DateRange != null ? src.DateRange.End : (LocalDate?)null)
            .Map(dest => dest.Portfolio, src => NavigationDto.Create(src.Portfolio!.Id, src.Portfolio.Key, src.Portfolio.Name))
            .Map(dest => dest.ProjectSponsors, src => src.Roles.Where(r => r.Role == ProjectRole.Sponsor).Select(x => EmployeeNavigationDto.From(x.Employee!)).ToList())
            .Map(dest => dest.ProjectOwners, src => src.Roles.Where(r => r.Role == ProjectRole.Owner).Select(x => EmployeeNavigationDto.From(x.Employee!)).ToList())
            .Map(dest => dest.ProjectManagers, src => src.Roles.Where(r => r.Role == ProjectRole.Manager).Select(x => EmployeeNavigationDto.From(x.Employee!)).ToList())
            .Map(dest => dest.StrategicThemes, src => src.StrategicThemeTags.Select(x => NavigationDto.Create(x.StrategicTheme!.Id, x.StrategicTheme.Key, x.StrategicTheme.Name)).ToList());
    }
}
