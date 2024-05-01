using Moda.Common.Application.Employees.Dtos;
using Moda.Work.Application.Workspaces.Dtos;

namespace Moda.Work.Application.WorkItems.Dtos;
public sealed record WorkItemDetailsDto : IMapFrom<WorkItem>
{
    public Guid Id { get; set; }
    public required string Key { get; set; }
    public int? ExternalId { get; set; }
    public required string Title { get; set; }
    public required WorkspaceNavigationDto Workspace { get; set; }
    public required string Type { get; set; }
    public required string Status { get; set; }
    public int? Priority { get; set; }
    public WorkItemNavigationDto? Parent { get; set; }
    public EmployeeNavigationDto? AssignedTo { get; set; }
    public Instant Created { get; private set; }
    public EmployeeNavigationDto? CreatedBy { get; set; }
    public Instant LastModified { get; private set; }
    public EmployeeNavigationDto? LastModifiedBy { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkItem, WorkItemDetailsDto>()
            .Map(dest => dest.Key, src => src.Key.ToString())
            .Map(dest => dest.Type, src => src.Type.Name)
            .Map(dest => dest.Status, src => src.Status.Name)
            .Map(dest => dest.AssignedTo, src => src.AssignedTo == null ? null : EmployeeNavigationDto.From(src.AssignedTo))
            .Map(dest => dest.CreatedBy, src => src.CreatedBy == null ? null : EmployeeNavigationDto.From(src.CreatedBy))
            .Map(dest => dest.LastModifiedBy, src => src.LastModifiedBy == null ? null : EmployeeNavigationDto.From(src.LastModifiedBy));
    }
}
