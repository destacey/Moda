using Moda.Common.Application.Dtos;
using Moda.Common.Application.Employees.Dtos;

namespace Moda.Work.Application.WorkItems.Dtos;
public sealed record DependencyDto : IMapFrom<WorkItemDependency>
{
    public Guid Id { get; set; }

    public required WorkItemDetailsNavigationDto Source { get; set; }

    public required WorkItemDetailsNavigationDto Target { get; set; }

    public required SimpleNavigationDto LinkType { get; set; }

    public required SimpleNavigationDto State { get; set; }

    public required SimpleNavigationDto Health { get; set; }

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
