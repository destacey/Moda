using Moda.Common.Application.Dtos;
using Moda.ProjectPortfolioManagement.Application.PpmTeams.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.ProjectTasks.Dtos;

/// <summary>
/// Lightweight DTO for a project task in list views.
/// </summary>
public sealed record ProjectTaskListDto : IMapFrom<ProjectTask>
{
    public Guid Id { get; set; }
    public int Key { get; set; }
    public required string TaskKey { get; set; }
    public Guid ProjectId { get; set; }
    public required string Name { get; set; }
    public required SimpleNavigationDto Type { get; set; }
    public required SimpleNavigationDto Status { get; set; }
    public SimpleNavigationDto? Priority { get; set; }
    public int Order { get; set; }
    public Guid? ParentId { get; set; }
    public ProjectTaskNavigationDto? Parent { get; set; }
    public PpmTeamNavigationDto? Team { get; set; }
    public LocalDate? PlannedStart { get; set; }
    public LocalDate? PlannedEnd { get; set; }
    public LocalDate? PlannedDate { get; set; }
    public LocalDate? ActualStart { get; set; }
    public LocalDate? ActualEnd { get; set; }
    public LocalDate? ActualDate { get; set; }
    public decimal? EstimatedEffortHours { get; set; }
    public decimal? ActualEffortHours { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<ProjectTask, ProjectTaskListDto>()
            .Map(dest => dest.TaskKey, src => src.TaskKey.Value)
            .Map(dest => dest.Type, src => SimpleNavigationDto.FromEnum(src.Type))
            .Map(dest => dest.Status, src => SimpleNavigationDto.FromEnum(src.Status))
            .Map(dest => dest.Priority, src => src.Priority.HasValue ? SimpleNavigationDto.FromEnum(src.Priority.Value) : null)
            .Map(dest => dest.PlannedStart, src => src.PlannedDateRange != null ? src.PlannedDateRange.Start : (LocalDate?)null)
            .Map(dest => dest.PlannedEnd, src => src.PlannedDateRange != null ? src.PlannedDateRange.End : null)
            .Map(dest => dest.ActualStart, src => src.ActualDateRange != null ? src.ActualDateRange.Start : (LocalDate?)null)
            .Map(dest => dest.ActualEnd, src => src.ActualDateRange != null ? src.ActualDateRange.End : null);
    }
}
