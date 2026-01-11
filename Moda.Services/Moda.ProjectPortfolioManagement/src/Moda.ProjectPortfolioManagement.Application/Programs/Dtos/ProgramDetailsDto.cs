using Moda.Common.Application.Dtos;
using Moda.Common.Application.Employees.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.Programs.Dtos;

public sealed record ProgramDetailsDto : IMapFrom<Program>
{
    public Guid Id { get; set; }

    /// <summary>
    /// The unique key of the program.  This is an alternate key to the Id.
    /// </summary>
    public int Key { get; set; }

    /// <summary>
    /// The name of the program.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// A detailed description of the programs's purpose and scope.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// The current status of the program.
    /// </summary>
    public required LifecycleNavigationDto Status { get; set; }

    /// <summary>
    /// The program start date.
    /// </summary>
    public LocalDate? Start { get; set; }

    /// <summary>
    /// The program end date.
    /// </summary>
    public LocalDate? End { get; set; }

    /// <summary>
    /// The portfolio associated with this program.
    /// </summary>
    public required NavigationDto Portfolio { get; set; }

    /// <summary>
    /// The sponsors of the program.
    /// </summary>
    public required List<EmployeeNavigationDto> ProgramSponsors { get; set; } = [];

    /// <summary>
    /// The owners of the program.
    /// </summary>
    public required List<EmployeeNavigationDto> ProgramOwners { get; set; } = [];

    /// <summary>
    /// The managers of the program.
    /// </summary>
    public required List<EmployeeNavigationDto> ProgramManagers { get; set; } = [];

    /// <summary>
    /// The strategic themes associated with this program.
    /// </summary>
    public required List<NavigationDto> StrategicThemes { get; set; } = [];

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<Program, ProgramDetailsDto>()
            .Map(dest => dest.Status, src => LifecycleNavigationDto.FromEnum(src.Status))
            .Map(dest => dest.Start, src => src.DateRange != null ? src.DateRange.Start : (LocalDate?)null)
            .Map(dest => dest.End, src => src.DateRange != null ? src.DateRange.End : (LocalDate?)null)
            .Map(dest => dest.Portfolio, src => NavigationDto.Create(src.Portfolio!.Id, src.Portfolio.Key, src.Portfolio.Name))
            .Map(dest => dest.ProgramSponsors, src => src.Roles.Where(r => r.Role == ProgramRole.Sponsor).Select(x => EmployeeNavigationDto.From(x.Employee!)).ToList())
            .Map(dest => dest.ProgramOwners, src => src.Roles.Where(r => r.Role == ProgramRole.Owner).Select(x => EmployeeNavigationDto.From(x.Employee!)).ToList())
            .Map(dest => dest.ProgramManagers, src => src.Roles.Where(r => r.Role == ProgramRole.Manager).Select(x => EmployeeNavigationDto.From(x.Employee!)).ToList())
            .Map(dest => dest.StrategicThemes, src => src.StrategicThemeTags.Select(x => NavigationDto.Create(x.StrategicTheme!.Id, x.StrategicTheme.Key, x.StrategicTheme.Name)).ToList());
    }
}
