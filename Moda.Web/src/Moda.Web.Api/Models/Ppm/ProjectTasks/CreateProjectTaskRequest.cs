using Moda.ProjectPortfolioManagement.Application.ProjectTasks.Commands;
using Moda.ProjectPortfolioManagement.Domain.Enums;

namespace Moda.Web.Api.Models.Ppm.ProjectTasks;

public sealed record CreateProjectTaskRequest
{
    /// <summary>
    /// The name of the task.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// A detailed description of the task (optional).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The type of task (Task or Milestone).
    /// </summary>
    public int TypeId { get; set; }

    /// <summary>
    /// The priority level of the task.
    /// </summary>
    public int PriorityId { get; set; }

    /// <summary>
    /// The ID of the parent task (optional).
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// The ID of the team assigned to this task (optional).
    /// </summary>
    public Guid? TeamId { get; set; }

    /// <summary>
    /// The planned start date for the task (for tasks, not milestones).
    /// </summary>
    public LocalDate? PlannedStart { get; set; }

    /// <summary>
    /// The planned end date for the task (for tasks, not milestones).
    /// </summary>
    public LocalDate? PlannedEnd { get; set; }

    /// <summary>
    /// The planned date for a milestone (for milestones only).
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

    public CreateProjectTaskCommand ToCreateProjectTaskCommand(Guid projectId)
    {
        var plannedDateRange = PlannedStart is null || PlannedEnd is null
            ? null
            : new FlexibleDateRange((LocalDate)PlannedStart, (LocalDate)PlannedEnd);

        var assignments = Assignments?
            .Select(a => new TaskRoleAssignment(a.EmployeeId, a.Role))
            .ToList();

        return new CreateProjectTaskCommand(
            projectId,
            Name,
            Description,
            (ProjectTaskType)TypeId,
            (TaskPriority)PriorityId,
            ParentId,
            TeamId,
            plannedDateRange,
            PlannedDate,
            EstimatedEffortHours,
            assignments
        );
    }
}

public sealed record TaskRoleAssignmentRequest
{
    /// <summary>
    /// The ID of the employee.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// The role of the assignment (Assignee or Reviewer).
    /// </summary>
    public TaskAssignmentRole Role { get; set; }
}

public sealed class CreateProjectTaskRequestValidator : CustomValidator<CreateProjectTaskRequest>
{
    public CreateProjectTaskRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Description)
            .MaximumLength(2048);

        RuleFor(x => x.TypeId)
            .Must(type => Enum.IsDefined(typeof(ProjectTaskType), type))
            .WithMessage("Invalid task type.");

        RuleFor(x => x.PriorityId)
            .Must(priority => Enum.IsDefined(typeof(TaskPriority), priority))
            .WithMessage("Invalid task priority.");

        RuleFor(x => x.ParentId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("ParentId cannot be an empty GUID.");

        RuleFor(x => x.TeamId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("TeamId cannot be an empty GUID.");

        // Milestone-specific validations
        RuleFor(x => x.PlannedDate)
            .NotNull()
            .When(x => x.TypeId == (int)ProjectTaskType.Milestone)
            .WithMessage("PlannedDate is required for milestones.");

        RuleFor(x => x.PlannedStart)
            .Must((request, plannedStart) => request.TypeId != (int)ProjectTaskType.Milestone || (request.PlannedStart == null && request.PlannedEnd == null))
            .WithMessage("Milestones cannot have a planned date range.");

        // Task-specific validations
        RuleFor(x => x.PlannedDate)
            .Null()
            .When(x => x.TypeId == (int)ProjectTaskType.Task)
            .WithMessage("Tasks cannot have a single planned date. Use PlannedStart and PlannedEnd instead.");

        RuleFor(x => x.PlannedStart)
            .Must((request, plannedStart) => (request.PlannedStart == null && request.PlannedEnd == null) || (request.PlannedStart != null && request.PlannedEnd != null))
            .When(x => x.TypeId == (int)ProjectTaskType.Task)
            .WithMessage("PlannedStart and PlannedEnd must either both be null or both have a value.");

        RuleFor(x => x.PlannedEnd)
            .Must((request, plannedEnd) => request.PlannedStart == null || request.PlannedEnd == null || request.PlannedStart <= request.PlannedEnd)
            .When(x => x.TypeId == (int)ProjectTaskType.Task)
            .WithMessage("PlannedEnd must be greater than or equal to PlannedStart.");

        RuleFor(x => x.EstimatedEffortHours)
            .GreaterThan(0)
            .When(x => x.EstimatedEffortHours.HasValue);

        RuleFor(x => x.Assignments)
            .Must(assignments => assignments == null || assignments.All(a => a.EmployeeId != Guid.Empty))
            .WithMessage("Assignment employee IDs cannot be empty GUIDs.");
    }
}
