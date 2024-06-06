using Moda.Common.Application.Dtos;
using Moda.Common.Application.Employees.Dtos;
using Moda.Work.Application.Workspaces.Dtos;

namespace Moda.Work.Application.WorkItems.Dtos;
public sealed record WorkItemListDto : IMapFrom<WorkItem>
{
    public Guid Id { get; set; }
    public required string Key { get; set; }
    public required string Title { get; set; }
    public required WorkspaceNavigationDto Workspace { get; set; }
    public required string Type { get; set; }
    public required string Status { get; set; }
    public required SimpleNavigationDto StatusCategory { get; set; }
    public WorkItemNavigationDto? Parent { get; set; }
    public EmployeeNavigationDto? AssignedTo { get; set; }
    public double StackRank { get; set; }
    public string? ExternalViewWorkItemUrl { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkItem, WorkItemListDto>()
            .Map(dest => dest.Key, src => src.Key.ToString())
            .Map(dest => dest.Type, src => src.Type.Name)
            .Map(dest => dest.Status, src => src.Status.Name)
            .Map(dest => dest.StatusCategory, src => SimpleNavigationDto.FromEnum(src.StatusCategory))
            .Map(dest => dest.AssignedTo, src => src.AssignedTo == null ? null : EmployeeNavigationDto.From(src.AssignedTo))
            .Map(dest => dest.ExternalViewWorkItemUrl, src => src.Workspace.ExternalViewWorkItemUrlTemplate == null ? null : $"{src.Workspace.ExternalViewWorkItemUrlTemplate}{src.ExternalId}");
    }
}

public static class WorkItemListDtoExtensions
{
    public static IEnumerable<WorkItemListDto> OrderByKey(this IEnumerable<WorkItemListDto> query, bool ascending)
    {
        return ascending
            ? query.OrderBy(x => x.Key[..x.Key.IndexOf('-')])
                   .ThenBy(x => int.Parse(x.Key[(x.Key.IndexOf('-') + 1)..]))
            : query.OrderByDescending(x => x.Key[..x.Key.IndexOf('-')])
                   .ThenByDescending(x => int.Parse(x.Key[(x.Key.IndexOf('-') + 1)..]));
    }
}
