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
    public EmployeeNavigationDto? AssignedTo { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkItem, WorkItemListDto>()
            .Map(dest => dest.Key, src => src.Key.ToString())
            .Map(dest => dest.Type, src => src.Type.Name)
            .Map(dest => dest.Status, src => src.Status.Name)
            .Map(dest => dest.AssignedTo, src => src.AssignedTo == null ? null : EmployeeNavigationDto.From(src.AssignedTo));
    }
}

public static class WorkItemListDtoExtensions
{
    public static IEnumerable<WorkItemListDto> OrderByKey(this IEnumerable<WorkItemListDto> query, bool ascending)
    {
        return ascending
            ? query.OrderBy(x => x.Key.Split('-')[0])
                   .ThenBy(x => int.Parse(x.Key.Split('-')[1]))
            : query.OrderByDescending(x => x.Key.Split('-')[0])
                   .ThenByDescending(x => int.Parse(x.Key.Split('-')[1]));
    }
}
