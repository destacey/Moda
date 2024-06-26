﻿namespace Moda.Work.Application.WorkItems.Dtos;
public sealed record WorkItemNavigationDto : IMapFrom<WorkItem>
{
    public required Guid Id { get; set; }
    public required string Key { get; set; }
    public required string Title { get; set; }
    public required string WorkspaceKey { get; set; }
    public string? ExternalViewWorkItemUrl { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkItem, WorkItemNavigationDto>()
            .Map(dest => dest.WorkspaceKey, src => src.Workspace.Key)
            .Map(dest => dest.ExternalViewWorkItemUrl, src => src.Workspace.ExternalViewWorkItemUrlTemplate == null ? null : $"{src.Workspace.ExternalViewWorkItemUrlTemplate}{src.ExternalId}"); ;
    }
}
