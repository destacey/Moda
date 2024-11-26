using Moda.Common.Application.Dtos;
using Moda.Common.Application.Employees.Dtos;
using Moda.Common.Domain.Extensions;

namespace Moda.Work.Application.WorkItems.Dtos;
public sealed record DependencyDto : IMapFrom<WorkItemLink>
{
    public Guid Id { get; set; }
    public required WorkItemDetailsNavigationDto Source { get; set; }
    public required WorkItemDetailsNavigationDto Target { get; set; }
    public required SimpleNavigationDto LinkType { get; set; }

    public SimpleNavigationDto Status 
        => SimpleNavigationDto.FromEnum(DependencyStatusExtensions.FromStatusCategoryString(Source.StatusCategory.Name));

    public Instant CreatedOn { get; set; }
    public EmployeeNavigationDto? CreatedBy { get; set; }
    public string? Comment { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkItemLink, DependencyDto>()
            .Map(dest => dest.LinkType, src => SimpleNavigationDto.FromEnum(src.LinkType))
            .Map(dest => dest.CreatedBy, src => src.CreatedBy == null ? null : EmployeeNavigationDto.From(src.CreatedBy))
            .Ignore(dest => dest.Status);
    }
}
