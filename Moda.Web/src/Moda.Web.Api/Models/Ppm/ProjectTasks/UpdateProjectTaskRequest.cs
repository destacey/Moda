using Moda.ProjectPortfolioManagement.Application.ProjectTasks.Commands;
using Moda.ProjectPortfolioManagement.Application.ProjectTasks.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using TaskStatus = Moda.ProjectPortfolioManagement.Domain.Enums.TaskStatus;

namespace Moda.Web.Api.Models.Ppm.ProjectTasks;

public sealed record UpdateProjectTaskRequest
{
    /// <summary>
    /// The ID of the task.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the task.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// A detailed description of the task (optional).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The current status of the task.
    /// </summary>
    public int StatusId { get; set; }

    /// <summary>
    /// The priority level of the task.
    /// </summary>
    public int PriorityId { get; set; }

    /// <summary>
    /// The progress of the task (optional). Ranges from 0.0 to 100.0. Milestones can not update progress directly.
    /// </summary>
    public decimal? Progress { get; set; }

    /// <summary>
    /// The ID of the parent task (optional).
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// The planned start date for the task.
    /// </summary>
    public LocalDate? PlannedStart { get; set; }

    /// <summary>
    /// The planned end date for the task.
    /// </summary>
    public LocalDate? PlannedEnd { get; set; }

    /// <summary>
    /// The planned date for a milestone.
    /// </summary>
    public LocalDate? PlannedDate { get; set; }

    /// <summary>
    /// The estimated effort in hours (optional).
    /// </summary>
    public decimal? EstimatedEffortHours { get; set; }

    /// <summary>
    /// The role-based assignments for this task (optional).
    /// </summary>
    public List<TaskRoleAssignmentRequest>? Assignments { get; set; }

    /// <summary>
    /// Creates an UpdateProjectTaskRequest from a ProjectTaskDto.
    /// </summary>
    public static UpdateProjectTaskRequest FromDto(ProjectTaskDto dto)
    {
        return new UpdateProjectTaskRequest
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            StatusId = dto.Status.Id,
            PriorityId = dto.Priority.Id,
            Progress = dto.Progress,
            ParentId = dto.ParentId,
            PlannedStart = dto.PlannedStart,
            PlannedEnd = dto.PlannedEnd,
            PlannedDate = dto.PlannedDate,
            EstimatedEffortHours = dto.EstimatedEffortHours,
            Assignments = dto.Assignments?
                .Select(a => new TaskRoleAssignmentRequest
                {
                    EmployeeId = a.Employee.Id,
                    Role = (TaskRole)a.Role.Id
                })
                .ToList()
        };
    }

    public UpdateProjectTaskCommand ToUpdateProjectTaskCommand()
    {
        var plannedDateRange = PlannedStart is null || PlannedEnd is null
            ? null
            : new FlexibleDateRange((LocalDate)PlannedStart, (LocalDate)PlannedEnd);

        var assignments = Assignments?
            .Select(a => new TaskRoleAssignment(a.EmployeeId, a.Role))
            .ToList();

        return new UpdateProjectTaskCommand(
            Id,
            Name,
            Description,
            (TaskStatus)StatusId,
            (TaskPriority)PriorityId,
            Progress.HasValue ? new Progress(Progress.Value) : null,
            ParentId,
            plannedDateRange,
            PlannedDate,
            EstimatedEffortHours,
            assignments
        );
    }
}

public sealed class UpdateProjectTaskRequestValidator : CustomValidator<UpdateProjectTaskRequest>
{
    public UpdateProjectTaskRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Description)
            .MaximumLength(2048);

        RuleFor(x => x.StatusId)
            .Must(status => Enum.IsDefined(typeof(TaskStatus), status))
            .WithMessage("Invalid task status.");

        RuleFor(x => x.PriorityId)
            .Must(priority => Enum.IsDefined(typeof(TaskPriority), priority))
            .WithMessage("Invalid task priority.");

        RuleFor(x => x.PlannedStart)
            .Must((request, plannedStart) => (request.PlannedStart == null && request.PlannedEnd == null) || (request.PlannedStart != null && request.PlannedEnd != null))
            .WithMessage("PlannedStart and PlannedEnd must either both be null or both have a value.");

        RuleFor(x => x.PlannedEnd)
            .Must((request, plannedEnd) => request.PlannedStart == null || request.PlannedEnd == null || request.PlannedStart <= request.PlannedEnd)
            .WithMessage("PlannedEnd must be greater than or equal to PlannedStart.");

        RuleFor(x => x.EstimatedEffortHours)
            .GreaterThan(0)
            .When(x => x.EstimatedEffortHours.HasValue);

        RuleFor(x => x.Assignments)
            .Must(assignments => assignments == null || assignments.All(a => a.EmployeeId != Guid.Empty))
            .WithMessage("Assignment employee IDs cannot be empty GUIDs.");
    }
}
