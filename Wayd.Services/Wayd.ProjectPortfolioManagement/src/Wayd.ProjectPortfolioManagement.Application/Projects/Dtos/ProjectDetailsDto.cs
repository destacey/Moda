using Wayd.Common.Application.Dtos;
using Wayd.Common.Application.Employees.Dtos;
using Wayd.ProjectPortfolioManagement.Domain.Enums;
using Wayd.ProjectPortfolioManagement.Domain.Models;

namespace Wayd.ProjectPortfolioManagement.Application.Projects.Dtos;

public sealed record ProjectDetailsDto : IMapFrom<Project>
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
    /// A concise summary of what the project delivers and its scope.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// The strategic justification for the project — why it should be funded.
    /// Captures the problem being solved or opportunity being pursued and the strategic rationale.
    /// </summary>
    public string? BusinessCase { get; set; }

    /// <summary>
    /// The specific, measurable outcomes expected upon successful delivery.
    /// Examples: revenue growth, cost savings, compliance achievement, efficiency improvements.
    /// </summary>
    public string? ExpectedBenefits { get; set; }

    /// <summary>
    /// The current status of the project.
    /// </summary>
    public required LifecycleNavigationDto Status { get; set; }

    /// <summary>
    /// The expenditure category associated with the project.
    /// </summary>
    public required SimpleNavigationDto ExpenditureCategory { get; set; }

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
    /// The members of the project team.
    /// </summary>
    public required List<EmployeeNavigationDto> ProjectMembers { get; set; } = [];

    /// <summary>
    /// The strategic themes associated with this project.
    /// </summary>
    public required List<NavigationDto> StrategicThemes { get; set; } = [];

    /// <summary>
    /// The project lifecycle assigned to this project.
    /// </summary>
    public DescriptiveNavigationDto? ProjectLifecycle { get; set; }

    /// <summary>
    /// The project phases, ordered by display order.
    /// </summary>
    public List<ProjectPhaseListDto> Phases { get; set; } = [];

    /// <summary>
    /// The current (non-expired) health check for this project, or null if none exists.
    /// Populated post-projection — not mapped by Mapster.
    /// </summary>
    public ProjectHealthCheckDto? HealthCheck { get; set; }

    /// <summary>
    /// Whether the current user can manage health checks for this project.
    /// Populated post-projection — not mapped by Mapster.
    /// </summary>
    public bool CanManageHealthChecks { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<Project, ProjectDetailsDto>()
            .Map(dest => dest.Key, src => src.Key.Value)
            .Map(dest => dest.Status, src => LifecycleNavigationDto.FromEnum(src.Status))
            .Map(dest => dest.ExpenditureCategory, src => SimpleNavigationDto.Create(src.ExpenditureCategory!.Id, src.ExpenditureCategory.Name))
            .Map(dest => dest.Start, src => src.DateRange != null ? src.DateRange.Start : (LocalDate?)null)
            .Map(dest => dest.End, src => src.DateRange != null ? src.DateRange.End : (LocalDate?)null)
            .Map(dest => dest.Portfolio, src => NavigationDto.Create(src.Portfolio!.Id, src.Portfolio.Key, src.Portfolio.Name))
            .Map(dest => dest.Program, src => src.Program != null ? NavigationDto.Create(src.Program.Id, src.Program.Key, src.Program.Name) : null)
            .Map(dest => dest.ProjectSponsors, src => src.Roles.Where(r => r.Role == ProjectRole.Sponsor).Select(x => EmployeeNavigationDto.From(x.Employee!)).ToList())
            .Map(dest => dest.ProjectOwners, src => src.Roles.Where(r => r.Role == ProjectRole.Owner).Select(x => EmployeeNavigationDto.From(x.Employee!)).ToList())
            .Map(dest => dest.ProjectManagers, src => src.Roles.Where(r => r.Role == ProjectRole.Manager).Select(x => EmployeeNavigationDto.From(x.Employee!)).ToList())
            .Map(dest => dest.ProjectMembers, src => src.Roles.Where(r => r.Role == ProjectRole.Member).Select(x => EmployeeNavigationDto.From(x.Employee!)).ToList())
            .Map(dest => dest.StrategicThemes, src => src.StrategicThemeTags.Select(x => NavigationDto.Create(x.StrategicTheme!.Id, x.StrategicTheme.Key, x.StrategicTheme.Name)).ToList())
            .Map(dest => dest.ProjectLifecycle, src => src.ProjectLifecycle != null ? DescriptiveNavigationDto.Create(src.ProjectLifecycle.Id, src.ProjectLifecycle.Key, src.ProjectLifecycle.Name, src.ProjectLifecycle.Description) : null)
            .Map(dest => dest.Phases, src => src.Phases.OrderBy(p => p.Order));
    }
}
