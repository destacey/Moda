using Moda.Common.Application.Dtos;
using Moda.Common.Application.Employees.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.ProjectTasks.Dtos;

/// <summary>
/// Hierarchical DTO for a project task with WBS and children for tree views.
/// </summary>
public sealed record ProjectTaskTreeDto : IMapFrom<ProjectTask>
{
    public Guid Id { get; set; }
    public required string Key { get; set; }
    public Guid ProjectId { get; set; }
    public required string Name { get; set; }
    public required SimpleNavigationDto Type { get; set; }
    public required SimpleNavigationDto Status { get; set; }
    public SimpleNavigationDto? Priority { get; set; }

    /// <summary>
    /// The progress of the task as a percentage (0 to 100).
    /// </summary>
    public decimal Progress { get; set; }
    public int Order { get; set; }
    public Guid? ParentId { get; set; }

    /// <summary>
    /// The Work Breakdown Structure (WBS) number (e.g., "1.2.3").
    /// </summary>
    public required string Wbs { get; set; }
    public List<ProjectTaskAssignmentDto> Assignments { get; set; } = [];
    public LocalDate? PlannedStart { get; set; }
    public LocalDate? PlannedEnd { get; set; }
    public LocalDate? PlannedDate { get; set; }
    public decimal? EstimatedEffortHours { get; set; }

    /// <summary>
    /// The child tasks.
    /// </summary>
    public List<ProjectTaskTreeDto> Children { get; set; } = [];

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<ProjectTask, ProjectTaskTreeDto>()
            .Map(dest => dest.Key, src => src.Key.Value)
            .Map(dest => dest.Type, src => SimpleNavigationDto.FromEnum(src.Type))
            .Map(dest => dest.Status, src => SimpleNavigationDto.FromEnum(src.Status))
            .Map(dest => dest.Priority, src => SimpleNavigationDto.FromEnum(src.Priority))
            .Map(dest => dest.Progress, src => src.Progress.Value)
            .Map(dest => dest.PlannedStart, src => src.PlannedDateRange != null ? src.PlannedDateRange.Start : (LocalDate?)null)
            .Map(dest => dest.PlannedEnd, src => src.PlannedDateRange != null ? src.PlannedDateRange.End : null)
            .Map(dest => dest.Assignments, src => src.Roles.Select(a => new ProjectTaskAssignmentDto
            {
                Employee = EmployeeNavigationDto.From(a.Employee!),
                Role = SimpleNavigationDto.FromEnum(a.Role)
            }).ToList())
            .Map(dest => dest.Wbs, src => string.Empty) // WBS will be calculated by WbsCalculator in the query handler
            .Map(dest => dest.Children, src => new List<ProjectTaskTreeDto>()); // Children will be populated in the query handler
    }
}
