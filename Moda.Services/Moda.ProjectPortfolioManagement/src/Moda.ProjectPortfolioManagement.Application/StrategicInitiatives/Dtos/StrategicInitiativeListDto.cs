using Moda.Common.Application.Dtos;
using Moda.Common.Application.Employees.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Dtos;

public sealed record StrategicInitiativeListDto : IMapFrom<StrategicInitiative>
{
    public Guid Id { get; set; }

    /// <summary>
    /// The unique key of the strategic initiative.  This is an alternate key to the Id.
    /// </summary>
    public int Key { get; set; }

    /// <summary>
    /// The name of the strategic initiative.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The status of the strategic initiative.
    /// </summary>
    public required SimpleNavigationDto Status { get; set; }

    /// <summary>
    /// The strategic initiative start date.
    /// </summary>
    public LocalDate? Start { get; set; }

    /// <summary>
    /// The strategic initiative end date.
    /// </summary>
    public LocalDate? End { get; set; }

    /// <summary>
    /// The portfolio associated with this strategic initiative.
    /// </summary>
    public required NavigationDto Portfolio { get; set; }

    /// <summary>
    /// The sponsors of the strategic initiative.
    /// </summary>
    public required List<EmployeeNavigationDto> StrategicInitiativeSponsors { get; set; } = [];

    /// <summary>
    /// The owners of the strategic initiative.
    /// </summary>
    public required List<EmployeeNavigationDto> StrategicInitiativeOwners { get; set; } = [];

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<StrategicInitiative, StrategicInitiativeListDto>()
            .Map(dest => dest.Status, src => SimpleNavigationDto.FromEnum(src.Status))
            .Map(dest => dest.Start, src => src.DateRange != null ? src.DateRange.Start : (LocalDate?)null)
            .Map(dest => dest.End, src => src.DateRange != null ? src.DateRange.End : (LocalDate?)null)
            .Map(dest => dest.Portfolio, src => NavigationDto.Create(src.Portfolio!.Id, src.Portfolio.Key, src.Portfolio.Name))
            .Map(dest => dest.StrategicInitiativeSponsors, src => src.Roles.Where(r => r.Role == StrategicInitiativeRole.Sponsor).Select(x => EmployeeNavigationDto.From(x.Employee!)).ToList())
            .Map(dest => dest.StrategicInitiativeOwners, src => src.Roles.Where(r => r.Role == StrategicInitiativeRole.Owner).Select(x => EmployeeNavigationDto.From(x.Employee!)).ToList());
    }
}
