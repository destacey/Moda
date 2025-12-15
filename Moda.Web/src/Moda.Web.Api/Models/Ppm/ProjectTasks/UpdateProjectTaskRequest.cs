using Moda.ProjectPortfolioManagement.Application.ProjectTasks.Commands;
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
    /// The priority level of the task (optional).
    /// </summary>
    public int? PriorityId { get; set; }

    /// <summary>
    /// The ID of the team assigned to this task (optional).
    /// </summary>
    public Guid? TeamId { get; set; }

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
    /// The actual start date when work began.
    /// </summary>
    public LocalDate? ActualStart { get; set; }

    /// <summary>
    /// The actual end date when work completed.
    /// </summary>
    public LocalDate? ActualEnd { get; set; }

    /// <summary>
    /// The actual date a milestone was achieved.
    /// </summary>
    public LocalDate? ActualDate { get; set; }

    /// <summary>
    /// The estimated effort in hours (optional).
    /// </summary>
    public decimal? EstimatedEffortHours { get; set; }

    /// <summary>
    /// The actual effort spent in hours (optional).
    /// </summary>
    public decimal? ActualEffortHours { get; set; }

    /// <summary>
    /// The role-based assignments for this task (optional).
    /// </summary>
    public List<TaskRoleAssignmentRequest>? Assignments { get; set; }

    public UpdateProjectTaskCommand ToUpdateProjectTaskCommand()
    {
        var plannedDateRange = PlannedStart is null || PlannedEnd is null
            ? null
            : new FlexibleDateRange((LocalDate)PlannedStart, (LocalDate)PlannedEnd);

        var actualDateRange = ActualStart is null || ActualEnd is null
            ? null
            : new FlexibleDateRange((LocalDate)ActualStart, (LocalDate)ActualEnd);

        var assignments = Assignments?
            .Select(a => new TaskRoleAssignment(a.EmployeeId, a.Role))
            .ToList();

        return new UpdateProjectTaskCommand(
            Id,
            Name,
            Description,
            (TaskStatus)StatusId,
            PriorityId.HasValue ? (TaskPriority)PriorityId.Value : null,
            TeamId,
            plannedDateRange,
            PlannedDate,
            actualDateRange,
            ActualDate,
            EstimatedEffortHours,
            ActualEffortHours,
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
            .Must(priority => Enum.IsDefined(typeof(TaskPriority), priority!.Value))
            .When(x => x.PriorityId.HasValue)
            .WithMessage("Invalid task priority.");

        RuleFor(x => x.TeamId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("TeamId cannot be an empty GUID.");

        RuleFor(x => x)
            .Must(x => (x.PlannedStart == null && x.PlannedEnd == null) || (x.PlannedStart != null && x.PlannedEnd != null))
            .WithMessage("PlannedStart and PlannedEnd must either both be null or both have a value.");

        RuleFor(x => x)
            .Must(x => x.PlannedStart == null || x.PlannedEnd == null || x.PlannedStart <= x.PlannedEnd)
            .WithMessage("PlannedEnd must be greater than or equal to PlannedStart.");

        RuleFor(x => x)
            .Must(x => (x.ActualStart == null && x.ActualEnd == null) || (x.ActualStart != null && x.ActualEnd != null))
            .WithMessage("ActualStart and ActualEnd must either both be null or both have a value.");

        RuleFor(x => x)
            .Must(x => x.ActualStart == null || x.ActualEnd == null || x.ActualStart <= x.ActualEnd)
            .WithMessage("ActualEnd must be greater than or equal to ActualStart.");

        RuleFor(x => x.EstimatedEffortHours)
            .GreaterThan(0)
            .When(x => x.EstimatedEffortHours.HasValue);

        RuleFor(x => x.ActualEffortHours)
            .GreaterThan(0)
            .When(x => x.ActualEffortHours.HasValue);

        RuleFor(x => x.Assignments)
            .Must(assignments => assignments == null || assignments.All(a => a.EmployeeId != Guid.Empty))
            .WithMessage("Assignment employee IDs cannot be empty GUIDs.");
    }
}
