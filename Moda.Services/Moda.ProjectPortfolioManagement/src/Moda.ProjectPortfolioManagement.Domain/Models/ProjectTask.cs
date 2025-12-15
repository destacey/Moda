using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Models.ProjectPortfolioManagement;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Interfaces;
using NodaTime;
using TaskStatus = Moda.ProjectPortfolioManagement.Domain.Enums.TaskStatus;

namespace Moda.ProjectPortfolioManagement.Domain.Models;

/// <summary>
/// Represents a task within a project with hierarchical structure and dependency management.
/// </summary>
public sealed class ProjectTask : BaseEntity<Guid>, ISystemAuditable, IHasIdAndKey, IHasProject
{
    private readonly List<ProjectTask> _children = [];
    private readonly HashSet<RoleAssignment<TaskAssignmentRole>> _assignments = [];
    private readonly List<ProjectTaskDependency> _predecessors = [];
    private readonly List<ProjectTaskDependency> _successors = [];

    private ProjectTask() { }

    private ProjectTask(
        Guid projectId,
        ProjectKey projectKey,
        int taskNumber,
        string name,
        string? description,
        ProjectTaskType type,
        TaskStatus status,
        TaskPriority? priority,
        int order,
        Guid? parentId,
        Guid? teamId,
        FlexibleDateRange? plannedDateRange,
        LocalDate? plannedDate,
        decimal? estimatedEffortHours,
        Dictionary<TaskAssignmentRole, HashSet<Guid>>? assignments)
    {
        // Validation
        if (type == ProjectTaskType.Milestone && plannedDate is null)
        {
            throw new InvalidOperationException("Milestones must have a planned date.");
        }

        // No need to validate date range - FlexibleDateRange constructor handles it

        ProjectId = projectId;
        TaskKey = new ProjectTaskKey(projectKey, taskNumber);
        Name = name;
        Description = description;
        Type = type;
        Status = status;
        Priority = priority;
        Order = order;
        ParentId = parentId;
        TeamId = teamId;
        PlannedDateRange = plannedDateRange;
        PlannedDate = plannedDate;
        EstimatedEffortHours = estimatedEffortHours;

        _assignments = assignments?
            .SelectMany(r => r.Value
                .Select(e => new RoleAssignment<TaskAssignmentRole>(Id, r.Key, e)))
            .ToHashSet()
            ?? [];
    }

    /// <summary>
    /// The unique integer key of the task. This is an alternate key to the Id.
    /// </summary>
    public int Key { get; private init; }

    /// <summary>
    /// The unique task key in the format {ProjectCode}-T{Number} (e.g., "APOLLO-T001").
    /// </summary>
    public ProjectTaskKey TaskKey { get; private init; } = null!;

    /// <summary>
    /// The ID of the project this task belongs to.
    /// </summary>
    public Guid ProjectId { get; private init; }

    /// <summary>
    /// The project this task belongs to.
    /// </summary>
    public Project Project { get; private set; }

    /// <summary>
    /// The name of the task.
    /// </summary>
    public string Name
    {
        get;
        private set => field = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    } = default!;

    /// <summary>
    /// A detailed description of the task.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// The type of task (Task or Milestone).
    /// </summary>
    public ProjectTaskType Type { get; private set; }

    /// <summary>
    /// The current status of the task.
    /// </summary>
    public TaskStatus Status { get; private set; }

    /// <summary>
    /// The priority level of the task.
    /// </summary>
    public TaskPriority? Priority { get; private set; }

    /// <summary>
    /// The order of the task within its parent (used for WBS calculation).
    /// </summary>
    public int Order { get; private set; }

    /// <summary>
    /// The ID of the parent task (null for root-level tasks).
    /// </summary>
    public Guid? ParentId { get; private set; }

    /// <summary>
    /// The parent task.
    /// </summary>
    public ProjectTask? Parent { get; private set; }

    /// <summary>
    /// The child tasks.
    /// </summary>
    public IReadOnlyList<ProjectTask> Children => _children.AsReadOnly();

    /// <summary>
    /// The ID of the team assigned to this task.
    /// </summary>
    public Guid? TeamId { get; private set; }

    /// <summary>
    /// The role-based assignments for this task.
    /// </summary>
    public IReadOnlyCollection<RoleAssignment<TaskAssignmentRole>> Assignments => _assignments;

    // Date properties for regular tasks
    /// <summary>
    /// The planned date range for the task. Only applicable for tasks (not milestones).
    /// </summary>
    public FlexibleDateRange? PlannedDateRange { get; private set; }

    /// <summary>
    /// The actual date range when the task was worked on. Only applicable for tasks (not milestones).
    /// End date is null until the task is completed.
    /// </summary>
    public FlexibleDateRange? ActualDateRange { get; private set; }

    // Date properties for milestones
    /// <summary>
    /// The planned date for a milestone. Only applicable for milestones (not tasks).
    /// </summary>
    public LocalDate? PlannedDate { get; private set; }

    /// <summary>
    /// The actual date a milestone was achieved. Only applicable for milestones (not tasks).
    /// </summary>
    public LocalDate? ActualDate { get; private set; }

    // Effort tracking
    /// <summary>
    /// The estimated effort in hours.
    /// </summary>
    public decimal? EstimatedEffortHours { get; private set; }

    /// <summary>
    /// The actual effort spent in hours.
    /// </summary>
    public decimal? ActualEffortHours { get; private set; }

    /// <summary>
    /// The dependencies where this task is the predecessor.
    /// </summary>
    public IReadOnlyList<ProjectTaskDependency> Successors => _successors.AsReadOnly();

    /// <summary>
    /// The dependencies where this task is the successor.
    /// </summary>
    public IReadOnlyList<ProjectTaskDependency> Predecessors => _predecessors.AsReadOnly();

    #region Domain Methods

    /// <summary>
    /// Updates the basic details of the task.
    /// </summary>
    public Result UpdateDetails(string name, string? description, TaskPriority? priority)
    {
        Name = name;
        Description = description;
        Priority = priority;
        return Result.Success();
    }

    /// <summary>
    /// Updates the status of the task and auto-sets actual dates based on status transitions.
    /// </summary>
    public Result UpdateStatus(TaskStatus status, Instant timestamp)
    {
        if (Type == ProjectTaskType.Milestone && status == TaskStatus.InProgress)
        {
            return Result.Failure("Milestones cannot be in progress. Use NotStarted or Completed.");
        }

        var previousStatus = Status;
        Status = status;

        // Auto-set actual dates based on status
        var currentDate = timestamp.InUtc().Date;

        if (status == TaskStatus.InProgress && previousStatus == TaskStatus.NotStarted && ActualDateRange is null)
        {
            if (Type == ProjectTaskType.Task)
            {
                // Start the task with open-ended range (no end date yet)
                ActualDateRange = new FlexibleDateRange(currentDate, null);
            }
        }

        if (status == TaskStatus.Completed)
        {
            if (Type == ProjectTaskType.Milestone)
            {
                ActualDate = currentDate;
            }
            else
            {
                if (ActualDateRange is not null)
                {
                    // Task was in progress, now complete it
                    ActualDateRange = new FlexibleDateRange(ActualDateRange.Start, currentDate);
                }
                else
                {
                    // Task completed without being marked in progress
                    ActualDateRange = new FlexibleDateRange(currentDate, currentDate);
                }
            }
        }

        return Result.Success();
    }

    /// <summary>
    /// Updates the planned dates for the task.
    /// </summary>
    public Result UpdatePlannedDates(FlexibleDateRange? plannedDateRange, LocalDate? plannedDate)
    {
        if (Type == ProjectTaskType.Milestone)
        {
            if (plannedDate is null)
            {
                return Result.Failure("Milestones must have a planned date.");
            }
            PlannedDate = plannedDate;
            PlannedDateRange = null;
        }
        else
        {
            PlannedDateRange = plannedDateRange;
            PlannedDate = null;
        }

        return Result.Success();
    }

    /// <summary>
    /// Updates the actual dates for the task.
    /// </summary>
    public Result UpdateActualDates(FlexibleDateRange? actualDateRange, LocalDate? actualDate)
    {
        if (Type == ProjectTaskType.Milestone)
        {
            ActualDate = actualDate;
            ActualDateRange = null;
        }
        else
        {
            ActualDateRange = actualDateRange;
            ActualDate = null;
        }

        return Result.Success();
    }

    /// <summary>
    /// Updates the effort estimates and actuals.
    /// </summary>
    public Result UpdateEffort(decimal? estimatedEffortHours, decimal? actualEffortHours)
    {
        if (estimatedEffortHours.HasValue && estimatedEffortHours < 0)
        {
            return Result.Failure("Estimated effort cannot be negative.");
        }

        if (actualEffortHours.HasValue && actualEffortHours < 0)
        {
            return Result.Failure("Actual effort cannot be negative.");
        }

        EstimatedEffortHours = estimatedEffortHours;
        ActualEffortHours = actualEffortHours;
        return Result.Success();
    }

    /// <summary>
    /// Assigns an employee to a specific role for this task.
    /// </summary>
    public Result AssignRole(TaskAssignmentRole role, Guid employeeId)
    {
        return RoleManager.AssignRole(_assignments, Id, role, employeeId);
    }

    /// <summary>
    /// Removes an employee from a specific role for this task.
    /// </summary>
    public Result RemoveRole(TaskAssignmentRole role, Guid employeeId)
    {
        return RoleManager.RemoveAssignment(_assignments, role, employeeId);
    }

    /// <summary>
    /// Updates all role assignments for this task.
    /// </summary>
    public Result UpdateRoles(Dictionary<TaskAssignmentRole, HashSet<Guid>> updatedRoles)
    {
        return RoleManager.UpdateRoles(_assignments, Id, updatedRoles);
    }

    /// <summary>
    /// Assigns a team to this task.
    /// </summary>
    public Result AssignTeam(Guid? teamId)
    {
        TeamId = teamId;
        return Result.Success();
    }

    /// <summary>
    /// Sets the order of this task within its parent.
    /// </summary>
    public Result SetOrder(int order)
    {
        if (order < 1)
        {
            return Result.Failure("Order must be greater than 0.");
        }

        Order = order;
        return Result.Success();
    }

    /// <summary>
    /// Changes the parent of this task.
    /// </summary>
    public Result ChangeParent(Guid? newParentId, ProjectTask? newParent)
    {
        // Validation: prevent circular references
        if (newParentId.HasValue && newParentId == Id)
        {
            return Result.Failure("A task cannot be its own parent.");
        }

        if (newParent is not null && Type == ProjectTaskType.Milestone)
        {
            return Result.Failure("Milestones cannot be moved under other tasks.");
        }

        if (newParent is not null && newParent.Type == ProjectTaskType.Milestone)
        {
            return Result.Failure("Milestones cannot have child tasks.");
        }

        // Check for circular reference by checking if newParent is a descendant of this task
        if (newParent is not null && IsDescendant(newParent.Id))
        {
            return Result.Failure("Cannot create a circular reference. The new parent is a descendant of this task.");
        }

        ParentId = newParentId;
        return Result.Success();
    }

    /// <summary>
    /// Adds a dependency where this task is the predecessor.
    /// </summary>
    public Result AddDependency(ProjectTask successor)
    {
        Guard.Against.Null(successor, nameof(successor));

        if (successor.Id == Id)
        {
            return Result.Failure("A task cannot depend on itself.");
        }

        if (successor.ProjectId != ProjectId)
        {
            return Result.Failure("Dependencies can only be created between tasks in the same project.");
        }

        // Check for existing active dependency
        if (_successors.Any(d => d.SuccessorId == successor.Id && d.IsActive))
        {
            return Result.Failure("An active dependency already exists with this task.");
        }

        // TODO: Check for circular dependencies in dependency graph

        var dependency = ProjectTaskDependency.Create(Id, successor.Id, DependencyType.FinishToStart);
        _successors.Add(dependency);

        return Result.Success();
    }

    /// <summary>
    /// Removes (soft deletes) a dependency where this task is the predecessor.
    /// </summary>
    public Result RemoveDependency(Guid successorId, Instant timestamp)
    {
        var dependency = _successors.FirstOrDefault(d => d.SuccessorId == successorId && d.IsActive);
        if (dependency is null)
        {
            return Result.Failure("Active dependency does not exist.");
        }

        dependency.Remove(timestamp);
        return Result.Success();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Adds a child task (called internally from parent task or project).
    /// </summary>
    internal void AddChild(ProjectTask child)
    {
        _children.Add(child);
    }

    /// <summary>
    /// Removes a child task (called internally).
    /// </summary>
    internal void RemoveChild(ProjectTask child)
    {
        _children.Remove(child);
    }

    /// <summary>
    /// Resets the order of all child tasks to eliminate gaps.
    /// </summary>
    internal void ResetChildrenOrder()
    {
        int i = 1;
        foreach (var child in _children.OrderBy(x => x.Order).ToArray())
        {
            child.Order = i;
            i++;
        }
    }

    /// <summary>
    /// Gets this task and all its descendants.
    /// </summary>
    internal List<Guid> GetSelfAndDescendants()
    {
        var ids = new List<Guid> { Id };

        foreach (var child in _children)
        {
            ids.AddRange(child.GetSelfAndDescendants());
        }

        return ids;
    }

    /// <summary>
    /// Checks if a given task ID is a descendant of this task.
    /// </summary>
    private bool IsDescendant(Guid taskId)
    {
        return _children.Any(c => c.Id == taskId || c.IsDescendant(taskId));
    }

    #endregion

    /// <summary>
    /// Creates a new project task (called from Project entity).
    /// </summary>
    internal static ProjectTask Create(
        Guid projectId,
        ProjectKey projectKey,
        int taskNumber,
        string name,
        string? description,
        ProjectTaskType type,
        TaskPriority? priority,
        int order,
        Guid? parentId,
        Guid? teamId,
        FlexibleDateRange? plannedDateRange,
        LocalDate? plannedDate,
        decimal? estimatedEffortHours,
        Dictionary<TaskAssignmentRole, HashSet<Guid>>? assignments)
    {
        return new ProjectTask(
            projectId,
            projectKey,
            taskNumber,
            name,
            description,
            type,
            TaskStatus.NotStarted,
            priority,
            order,
            parentId,
            teamId,
            plannedDateRange,
            plannedDate,
            estimatedEffortHours,
            assignments);
    }
}
