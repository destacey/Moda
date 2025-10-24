using Moda.Common.Application.Dtos;
using Moda.Common.Application.Employees.Dtos;
using Moda.Work.Application.WorkProjects.Dtos;
using Moda.Work.Application.Workspaces.Dtos;
using Moda.Work.Application.WorkTeams.Dtos;

namespace Moda.Work.Application.WorkItems.Dtos;
public sealed record WorkItemDetailsDto : IMapFrom<WorkItem>
{
    public Guid Id { get; set; }
    public required string Key { get; set; }
    public int? ExternalId { get; set; }
    public required string Title { get; set; }
    public required WorkspaceNavigationDto Workspace { get; set; }
    public required string Type { get; set; }
    public required string Tier { get; set; }
    public required string Status { get; set; }
    public required SimpleNavigationDto StatusCategory { get; set; }
    public int? Priority { get; set; }
    public WorkItemNavigationDto? Parent { get; set; }
    public WorkTeamNavigationDto? Team { get; set; }
    public EmployeeNavigationDto? AssignedTo { get; set; }
    public Instant Created { get; set; }
    public EmployeeNavigationDto? CreatedBy { get; set; }
    public Instant LastModified { get; set; }
    public EmployeeNavigationDto? LastModifiedBy { get; set; }
    public Instant? ActivatedTimestamp { get; set; }
    public Instant? DoneTimestamp { get; set; }
    public WorkProjectNavigationDto? Project { get; set; }
    public string? ExternalViewWorkItemUrl { get; set; }
    public double? StoryPoints { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkItem, WorkItemDetailsDto>()
            .Map(dest => dest.Key, src => src.Key.ToString())
            .Map(dest => dest.Type, src => src.Type.Name)
            .Map(dest => dest.Tier, src => src.Type.Level!.Tier.GetDisplayName())
            .Map(dest => dest.Status, src => src.Status.Name)
            .Map(dest => dest.StatusCategory, src => SimpleNavigationDto.FromEnum(src.StatusCategory))
            .Map(dest => dest.AssignedTo, src => src.AssignedTo == null ? null : EmployeeNavigationDto.From(src.AssignedTo))
            .Map(dest => dest.CreatedBy, src => src.CreatedBy == null ? null : EmployeeNavigationDto.From(src.CreatedBy))
            .Map(dest => dest.LastModifiedBy, src => src.LastModifiedBy == null ? null : EmployeeNavigationDto.From(src.LastModifiedBy))
            .Map(dest => dest.Project, src => src.Project != null
                ? WorkProjectNavigationDto.From(src.Project)
                : src.ParentProject != null
                    ? WorkProjectNavigationDto.From(src.ParentProject)
                    : null)
            .Map(dest => dest.ExternalViewWorkItemUrl, src => src.Workspace.ExternalViewWorkItemUrlTemplate == null ? null : $"{src.Workspace.ExternalViewWorkItemUrlTemplate}{src.ExternalId}");
    }
}
