using Moda.Common.Application.Dtos;
using Moda.Common.Application.Employees.Dtos;

namespace Moda.Work.Application.WorkItems.Dtos;
public sealed record WorkItemLinkDto : IMapFrom<WorkItemLink>
{
    public required WorkItemDetailsNavigationDto Source { get; set; }
    public required WorkItemDetailsNavigationDto Target { get; set; }
    public required SimpleNavigationDto LinkType { get; set; }
    public Instant CreatedOn { get; set; }
    public EmployeeNavigationDto? CreatedBy { get; set; }
    public Instant? RemovedOn { get; set; }
    public EmployeeNavigationDto? RemovedBy { get; set; }
    public string? Comment { get; set; }


    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkItemLink, WorkItemLinkDto>()
            .Map(dest => dest.LinkType, src => SimpleNavigationDto.FromEnum(src.LinkType))
            .Map(dest => dest.CreatedBy, src => src.CreatedBy == null ? null : EmployeeNavigationDto.From(src.CreatedBy))
            .Map(dest => dest.RemovedBy, src => src.RemovedBy == null ? null : EmployeeNavigationDto.From(src.RemovedBy));
    }
}
