﻿using Moda.Common.Application.Dtos;
using Moda.Common.Application.Employees.Dtos;
using Moda.Work.Application.WorkProjects.Dtos;
using Moda.Work.Application.Workspaces.Dtos;
using Moda.Work.Application.WorkTeams.Dtos;

namespace Moda.Work.Application.WorkItems.Dtos;

public sealed record WorkItemBacklogItemDto : IMapFrom<WorkItem>
{
    public Guid Id { get; set; }
    public required string Key { get; set; }
    public required string Title { get; set; }
    public required WorkspaceNavigationDto Workspace { get; set; }
    public required string Type { get; set; }
    public required string Status { get; set; }
    public required SimpleNavigationDto StatusCategory { get; set; }
    public WorkItemNavigationDto? Parent { get; set; }
    public WorkTeamNavigationDto? Team { get; set; }
    public EmployeeNavigationDto? AssignedTo { get; set; }
    public Instant Created { get; set; }
    public int Rank { get; set; }
    public int? ParentRank { get; set; }
    public WorkProjectNavigationDto? Project { get; set; }
    public string? ExternalViewWorkItemUrl { get; set; }

    // This is used to set the rank of the work items in the backlog
    public double StackRank { get; set; }
    public double? StoryPoints { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkItem, WorkItemBacklogItemDto>()
            .Map(dest => dest.Key, src => src.Key.ToString())
            .Map(dest => dest.Type, src => src.Type.Name)
            .Map(dest => dest.Status, src => src.Status.Name)
            .Map(dest => dest.StatusCategory, src => SimpleNavigationDto.FromEnum(src.StatusCategory))
            .Map(dest => dest.AssignedTo, src => src.AssignedTo == null ? null : EmployeeNavigationDto.From(src.AssignedTo))
            .Map(dest => dest.Project, src => src.Project != null
                ? WorkProjectNavigationDto.From(src.Project)
                : src.ParentProject != null
                    ? WorkProjectNavigationDto.From(src.ParentProject)
                    : null)
            .Map(dest => dest.ExternalViewWorkItemUrl, src => src.Workspace.ExternalViewWorkItemUrlTemplate == null ? null : $"{src.Workspace.ExternalViewWorkItemUrlTemplate}{src.ExternalId}");
    }
}
