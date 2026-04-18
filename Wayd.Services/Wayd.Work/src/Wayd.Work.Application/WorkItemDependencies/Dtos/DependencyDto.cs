using Wayd.Common.Application.Dtos;
using Wayd.Common.Application.Employees.Dtos;
using Wayd.Common.Domain.Enums.Work;
using Wayd.Work.Application.WorkItems.Dtos;

namespace Wayd.Work.Application.WorkItemDependencies.Dtos;

public sealed record DependencyDto : IMapFrom<WorkItemDependency>
{
    public Guid Id { get; set; }

    public required WorkItemDetailsNavigationDto Source { get; set; }

    public required WorkItemDetailsNavigationDto Target { get; set; }

    public required SimpleNavigationDto LinkType { get; set; }

    public required SimpleNavigationDto State { get; set; }

    public required SimpleNavigationDto Health { get; set; }

    public SimpleNavigationDto Scope =>
        Source.Team != null && Target.Team != null
        ? Source.Team.Id == Target.Team.Id
            ? SimpleNavigationDto.FromEnum(DependencyScope.IntraTeam)
            : SimpleNavigationDto.FromEnum(DependencyScope.CrossTeam)
        : SimpleNavigationDto.FromEnum(DependencyScope.Unknown);

    public Instant CreatedOn { get; set; }

    public EmployeeNavigationDto? CreatedBy { get; set; }

    public string? Comment { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkItemDependency, DependencyDto>()
            .Map(dest => dest.LinkType, src => SimpleNavigationDto.FromEnum(src.LinkType))
            .Map(dest => dest.State, src => SimpleNavigationDto.FromEnum(src.State))
            .Map(dest => dest.Health, src => SimpleNavigationDto.FromEnum(src.Health))
            .Map(dest => dest.CreatedBy, src => src.CreatedBy == null ? null : EmployeeNavigationDto.From(src.CreatedBy));
    }
}
