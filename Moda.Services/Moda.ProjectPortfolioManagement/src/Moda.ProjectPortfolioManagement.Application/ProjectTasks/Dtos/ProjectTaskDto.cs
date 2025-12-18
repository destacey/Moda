using Moda.Common.Application.Dtos;
using Moda.Common.Application.Employees.Dtos;
using Moda.ProjectPortfolioManagement.Application.PpmTeams.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.ProjectTasks.Dtos;

/// <summary>
/// Detailed DTO for a project task with full information including assignments.
/// </summary>
public sealed record ProjectTaskDto : IMapFrom<ProjectTask>
{
    /// <summary>
    /// The unique identifier of the task.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The unique key of the task. This is an alternate key to the Id.
    /// </summary>
    public int Key { get; set; }

    /// <summary>
    /// The task key in the format {ProjectCode}-T{Number} (e.g., "APOLLO-T001").
    /// </summary>
    public required string TaskKey { get; set; }

    /// <summary>
    /// The unique identifier of the parent project.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// The name of the task.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// A detailed description of the task.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The task type (Task or Milestone).
    /// </summary>
    public required SimpleNavigationDto Type { get; set; }

    /// <summary>
    /// The current status of the task.
    /// </summary>
    public required SimpleNavigationDto Status { get; set; }

    /// <summary>
    /// The priority of the task.
    /// </summary>
    public SimpleNavigationDto? Priority { get; set; }

    /// <summary>
    /// The order of the task within its parent.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// The unique identifier of the parent task.
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// The parent task navigation information.
    /// </summary>
    public ProjectTaskNavigationDto? Parent { get; set; }

    /// <summary>
    /// The team assigned to this task.
    /// </summary>
    public PpmTeamNavigationDto? Team { get; set; }

    /// <summary>
    /// The individual role assignments for this task.
    /// </summary>
    public List<ProjectTaskAssignmentDto> Assignments { get; set; } = [];

    /// <summary>
    /// The planned start date for regular tasks.
    /// </summary>
    public LocalDate? PlannedStart { get; set; }

    /// <summary>
    /// The planned end date for regular tasks.
    /// </summary>
    public LocalDate? PlannedEnd { get; set; }

    /// <summary>
    /// The planned date for milestones.
    /// </summary>
    public LocalDate? PlannedDate { get; set; }

    /// <summary>
    /// The actual start date for regular tasks.
    /// </summary>
    public LocalDate? ActualStart { get; set; }

    /// <summary>
    /// The actual end date for regular tasks.
    /// </summary>
    public LocalDate? ActualEnd { get; set; }

    /// <summary>
    /// The actual date for milestones.
    /// </summary>
    public LocalDate? ActualDate { get; set; }

    /// <summary>
    /// The estimated effort in hours.
    /// </summary>
    public decimal? EstimatedEffortHours { get; set; }

    /// <summary>
    /// The actual effort in hours.
    /// </summary>
    public decimal? ActualEffortHours { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<ProjectTask, ProjectTaskDto>()
            .Map(dest => dest.TaskKey, src => src.TaskKey.Value)
            .Map(dest => dest.Type, src => SimpleNavigationDto.FromEnum(src.Type))
            .Map(dest => dest.Status, src => SimpleNavigationDto.FromEnum(src.Status))
            .Map(dest => dest.Priority, src => SimpleNavigationDto.FromEnum(src.Priority))
            .Map(dest => dest.PlannedStart, src => src.PlannedDateRange != null ? src.PlannedDateRange.Start : (LocalDate?)null)
            .Map(dest => dest.PlannedEnd, src => src.PlannedDateRange != null ? src.PlannedDateRange.End : null)
            .Map(dest => dest.ActualStart, src => src.ActualDateRange != null ? src.ActualDateRange.Start : (LocalDate?)null)
            .Map(dest => dest.ActualEnd, src => src.ActualDateRange != null ? src.ActualDateRange.End : null)
            .Map(dest => dest.Assignments, src => src.Assignments.Select(a => new ProjectTaskAssignmentDto
            {
                EmployeeId = a.EmployeeId,
                Employee = EmployeeNavigationDto.From(a.Employee!),
                Role = SimpleNavigationDto.FromEnum(a.Role)
            }).ToList());
    }
}

/// <summary>
/// DTO for a task assignment (employee + role).
/// </summary>
public sealed record ProjectTaskAssignmentDto
{
    public Guid EmployeeId { get; set; }
    public required EmployeeNavigationDto Employee { get; set; }
    public required SimpleNavigationDto Role { get; set; }
}
