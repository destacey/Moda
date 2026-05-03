using Wayd.Common.Application.Dtos;
using Wayd.Common.Application.Employees.Dtos;
using Wayd.ProjectPortfolioManagement.Domain.Enums;
using Wayd.ProjectPortfolioManagement.Domain.Models;

namespace Wayd.ProjectPortfolioManagement.Application.Projects.Dtos;

public sealed record ProjectListDto
{
    public Guid Id { get; set; }

    /// <summary>
    /// The unique key of the project.  This is an alternate key to the Id.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// The name of the project.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The current status of the project.
    /// </summary>
    public required LifecycleNavigationDto Status { get; set; }

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
    /// The program associated with this project.
    /// </summary>
    public NavigationDto? Program { get; set; }

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
    /// The members of the project.
    /// </summary>
    public required List<EmployeeNavigationDto> ProjectMembers { get; set; } = [];

    /// <summary>
    /// The strategic themes associated with this project.
    /// </summary>
    public required List<NavigationDto> StrategicThemes { get; set; } = [];

    /// <summary>
    /// The project lifecycle assigned to this project.
    /// </summary>
    public NavigationDto? ProjectLifecycle { get; set; }

    /// <summary>
    /// The project phases, ordered by display order.
    /// </summary>
    public List<ProjectPhaseListDto> Phases { get; set; } = [];

    public ProjectHealthCheckSummaryDto? HealthCheck { get; set; }

    /// <summary>
    /// Create a TypeAdapterConfig configured to map Project -> ProjectListDto using the provided
    /// current time to filter health checks by expiration. Callers can pass this config to
    /// ProjectToType to perform an EF-friendly projection that uses a captured constant time.
    /// </summary>
    public static TypeAdapterConfig CreateTypeAdapterConfig(Instant now)
    {
        var config = new TypeAdapterConfig();

        config.NewConfig<Project, ProjectListDto>()
            .Map(dest => dest.Key, src => src.Key.Value)
            .Map(dest => dest.Status, src => LifecycleNavigationDto.FromEnum(src.Status))
            .Map(dest => dest.Start, src => src.DateRange != null ? src.DateRange.Start : (LocalDate?)null)
            .Map(dest => dest.End, src => src.DateRange != null ? src.DateRange.End : (LocalDate?)null)
            .Map(dest => dest.Portfolio, src => NavigationDto.Create(src.Portfolio!.Id, src.Portfolio.Key, src.Portfolio.Name))
            .Map(dest => dest.Program, src => src.Program != null ? NavigationDto.Create(src.Program.Id, src.Program.Key, src.Program.Name) : null)
            .Map(dest => dest.ProjectSponsors, src => src.Roles.Where(r => r.Role == ProjectRole.Sponsor).Select(x => EmployeeNavigationDto.From(x.Employee!)).ToList())
            .Map(dest => dest.ProjectOwners, src => src.Roles.Where(r => r.Role == ProjectRole.Owner).Select(x => EmployeeNavigationDto.From(x.Employee!)).ToList())
            .Map(dest => dest.ProjectManagers, src => src.Roles.Where(r => r.Role == ProjectRole.Manager).Select(x => EmployeeNavigationDto.From(x.Employee!)).ToList())
            .Map(dest => dest.ProjectMembers, src => src.Roles.Where(r => r.Role == ProjectRole.Member).Select(x => EmployeeNavigationDto.From(x.Employee!)).ToList())
            .Map(dest => dest.StrategicThemes, src => src.StrategicThemeTags.Select(x => NavigationDto.Create(x.StrategicTheme!.Id, x.StrategicTheme.Key, x.StrategicTheme.Name)).ToList())
            .Map(dest => dest.ProjectLifecycle, src => src.ProjectLifecycle != null ? NavigationDto.Create(src.ProjectLifecycle.Id, src.ProjectLifecycle.Key, src.ProjectLifecycle.Name) : null)
            .Map(dest => dest.Phases, src => src.Phases.OrderBy(p => p.Order))
            .Map(dest => dest.HealthCheck, src => src.HealthChecks
                .Where(h => !h.IsDeleted && h.Expiration > now)
                .Select(h => new ProjectHealthCheckSummaryDto
                {
                    Id = h.Id,
                    Status = SimpleNavigationDto.FromEnum(h.Status)
                })
                .FirstOrDefault());

        return config;
    }
}
