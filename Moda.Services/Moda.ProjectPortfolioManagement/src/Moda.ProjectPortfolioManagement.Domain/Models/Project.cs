using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Events.ProjectPortfolioManagement;
using Moda.Common.Domain.Interfaces.ProjectPortfolioManagement;
using Moda.Common.Domain.Models.ProjectPortfolioManagement;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;
using NodaTime;

namespace Moda.ProjectPortfolioManagement.Domain.Models;

/// <summary>
/// Represents an individual project within a portfolio or program.
/// </summary>
public sealed class Project : BaseEntity<Guid>, ISystemAuditable, IHasIdAndKey<ProjectKey>, ISimpleProject
{
    private readonly HashSet<RoleAssignment<ProjectRole>> _roles = [];
    private readonly HashSet<StrategicThemeTag<Project>> _strategicThemeTags = [];
    private readonly HashSet<StrategicInitiativeProject> _strategicInitiativeProjects = [];
    private readonly List<ProjectTask> _tasks = [];

    private Project() { }

    private Project(string name, string description, ProjectKey key, ProjectStatus status, int expenditureCategoryId, LocalDateRange? dateRange, Guid portfolioId, Guid? programId = null, Dictionary<ProjectRole, HashSet<Guid>>? roles = null, HashSet<Guid>? strategicThemes = null)
    {
        if (Status is ProjectStatus.Active or ProjectStatus.Completed && dateRange is null)
        {
            throw new InvalidOperationException("An active and completed project must have a start and end date.");
        }

        Name = name;
        Description = description;
        Key = key;
        Status = status;
        ExpenditureCategoryId = expenditureCategoryId;
        DateRange = dateRange;

        PortfolioId = portfolioId;
        ProgramId = programId;

        _roles = roles?
            .SelectMany(r => r.Value
                .Select(e => new RoleAssignment<ProjectRole>(Id, r.Key, e)))
            .ToHashSet()
            ?? [];

        _strategicThemeTags = strategicThemes?.Select(t => new StrategicThemeTag<Project>(Id, t)).ToHashSet()
            ?? [];
    }

    /// <summary>
    /// The unique key of the project used for task key generation (e.g., "APOLLO", "MARS").
    /// This is an alternate key to the Id.
    /// Must be 2-20 uppercase alphanumeric characters or hyphens.
    /// </summary>
    public ProjectKey Key { get; private set; } = default!;

    /// <summary>
    /// The name of the project.
    /// </summary>
    public string Name
    {
        get;
        private set => field = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    } = default!;

    /// <summary>
    /// A detailed description of the project's purpose and scope.
    /// </summary>
    public string Description
    {
        get;
        private set => field = Guard.Against.NullOrWhiteSpace(value, nameof(Description)).Trim();
    } = default!;

    /// <summary>
    /// The current status of the project.
    /// </summary>
    public ProjectStatus Status { get; private set; }

    /// <summary>
    /// The roles associated with the project.
    /// </summary>
    public IReadOnlyCollection<RoleAssignment<ProjectRole>> Roles => _roles;

    /// <summary>
    /// The date range defining the project's planned timeline.
    /// </summary>
    public LocalDateRange? DateRange { get; private set; }

    /// <summary>
    /// The Id of the expenditure category associated with the project.
    /// </summary>
    public int ExpenditureCategoryId { get; private set; }

    /// <summary>
    /// The expenditure category associated with the project.
    /// </summary>
    public ExpenditureCategory? ExpenditureCategory { get; private set; }

    /// <summary>
    /// The Id of the portfolio to which this project belongs.
    /// </summary>
    public Guid PortfolioId { get; private set; }

    /// <summary>
    /// The portfolio associated with this project.
    /// </summary>
    public ProjectPortfolio? Portfolio { get; private set; }

    /// <summary>
    /// The Id of the program to which this project belongs (optional).
    /// </summary>
    public Guid? ProgramId { get; private set; }

    /// <summary>
    /// The program associated with this project (if any).
    /// </summary>
    public Program? Program { get; private set; }

    /// <summary>
    /// Indicates if the project is in a closed state.
    /// </summary>
    public bool IsClosed => Status is ProjectStatus.Completed or ProjectStatus.Cancelled;

    /// <summary>
    /// The strategic theme tags associated with this project.
    /// </summary>
    public IReadOnlyCollection<StrategicThemeTag<Project>> StrategicThemeTags => _strategicThemeTags;

    /// <summary>
    /// The strategic initiatives associated with this project.
    /// </summary>
    public IReadOnlyCollection<StrategicInitiativeProject> StrategicInitiativeProjects => _strategicInitiativeProjects;

    /// <summary>
    /// The tasks associated with this project.
    /// </summary>
    public IReadOnlyCollection<ProjectTask> Tasks => _tasks.AsReadOnly();

    /// <summary>
    /// Indicates whether the project can be deleted.
    /// </summary>
    /// <returns></returns>
    public bool CanBeDeleted() => Status is ProjectStatus.Proposed;

    /// <summary>
    /// Updates the core details of the project.
    /// </summary>
    /// <param name="name">The new name to assign to the project. Cannot be null.</param>
    /// <param name="description">The new description to assign to the project. Cannot be null.</param>
    /// <param name="expenditureCategoryId">The new expenditure category ID to assign to the project.</param>
    /// <param name="timestamp">The timestamp indicating when the update occurred.</param>
    /// <returns></returns>
    public Result UpdateDetails(string name, string description, int expenditureCategoryId, Instant timestamp)
    {
        Name = name;
        Description = description;
        ExpenditureCategoryId = expenditureCategoryId;

        AddDomainEvent(new ProjectDetailsUpdatedEvent(this, ExpenditureCategoryId, timestamp));

        return Result.Success();
    }

    /// <summary>
    /// Assigns an employee to a specific role within the project, allowing multiple employees per role.
    /// </summary>
    public Result AssignRole(ProjectRole role, Guid employeeId)
    {
        return RoleManager.AssignRole(_roles, Id, role, employeeId);
    }

    /// <summary>
    /// Removes an employee from a specific role.
    /// </summary>
    public Result RemoveRole(ProjectRole role, Guid employeeId)
    {
        return RoleManager.RemoveAssignment(_roles, role, employeeId);
    }

    /// <summary>
    /// Updates the roles for the project.
    /// </summary>
    /// <param name="updatedRoles"></param>
    /// <returns></returns>
    public Result UpdateRoles(Dictionary<ProjectRole, HashSet<Guid>> updatedRoles)
    {
        return RoleManager.UpdateRoles(_roles, Id, updatedRoles);
    }

    public Result UpdateTimeline(LocalDateRange? dateRange)
    {
        if (Status is ProjectStatus.Active or ProjectStatus.Completed && dateRange is null)
        {
            return Result.Failure("Active and completed projects must have a start and end date.");
        }

        DateRange = dateRange;

        return Result.Success();
    }

    /// <summary>
    /// Updates the project's program association.
    /// </summary>
    /// <param name="program"></param>
    /// <returns></returns>
    internal Result UpdateProgram(Program? program)
    {
        if (program is null)
        {
            ProgramId = null;
            return Result.Success();
        }

        if (program.PortfolioId != PortfolioId)
        {
            return Result.Failure("The project must belong to the same portfolio as the program.");
        }

        ProgramId = program.Id;

        return Result.Success();
    }

    /// <summary>
    /// Associates a strategic theme with this project.
    /// </summary>
    public Result AddStrategicTheme(Guid strategicThemeId)
    {
        Guard.Against.NullOrEmpty(strategicThemeId, nameof(strategicThemeId));

        return StrategicThemeTagManager<Project>.AddStrategicThemeTag(_strategicThemeTags, Id, strategicThemeId, "project");
    }

    /// <summary>
    /// Removes a strategic theme from this project.
    /// </summary>
    public Result RemoveStrategicTheme(Guid strategicThemeId)
    {
        Guard.Against.NullOrEmpty(strategicThemeId, nameof(strategicThemeId));

        return StrategicThemeTagManager<Project>.RemoveStrategicThemeTag(_strategicThemeTags, strategicThemeId, "project");
    }

    /// <summary>
    /// Updates the strategic themes associated with this project.
    /// </summary>
    /// <param name="strategicThemeIds"></param>
    /// <returns></returns>
    public Result UpdateStrategicThemes(HashSet<Guid> strategicThemeIds)
    {
        Guard.Against.Null(strategicThemeIds, nameof(strategicThemeIds));

        return StrategicThemeTagManager<Project>.UpdateTags(_strategicThemeTags, Id, strategicThemeIds, "project");
    }

    #region Lifecycle

    /// <summary>
    /// Activates the project.
    /// </summary>
    public Result Activate()
    {
        if (Status != ProjectStatus.Proposed)
        {
            return Result.Failure("Only proposed projects can be activated.");
        }

        if (DateRange is null)
        {
            return Result.Failure("The project must have a start and end date before it can be activated.");
        }

        Status = ProjectStatus.Active;

        return Result.Success();
    }

    /// <summary>
    /// Marks the project as completed.
    /// </summary>
    public Result Complete()
    {
        if (Status != ProjectStatus.Active)
        {
            return Result.Failure("Only active projects can be completed.");
        }

        if (DateRange == null)
        {
            return Result.Failure("The project must have a start and end date before it can be completed.");
        }

        Status = ProjectStatus.Completed;

        return Result.Success();
    }

    /// <summary>
    /// Cancels the project.
    /// </summary>
    public Result Cancel()
    {
        if (Status is ProjectStatus.Completed or ProjectStatus.Cancelled)
        {
            return Result.Failure("The project is already completed or cancelled.");
        }

        Status = ProjectStatus.Cancelled;

        return Result.Success();
    }

    #endregion Lifecycle

    /// <summary>
    /// Checks if the portfolio is active on the specified date.
    /// </summary>
    public bool IsActiveOn(LocalDate date)
    {
        Guard.Against.Null(date, nameof(date));

        return DateRange is not null && DateRange.IsActiveOn(date);
    }

    #region Tasks

    /// <summary>
    /// Creates a new project task with the specified details and adds it to the project.
    /// </summary>
    /// <remarks>The project must have a key assigned before tasks can be created. If a parent task is
    /// specified, it must exist and cannot be a milestone, as milestones cannot have child tasks. The new task is
    /// assigned an order value based on its siblings under the same parent.</remarks>
    /// <param name="nextNumber">The next available task number to assign to the new task. Must be unique within the project.</param>
    /// <param name="name">The name of the task. Cannot be null or empty.</param>
    /// <param name="description">An optional description providing additional details about the task.</param>
    /// <param name="type">The type of the task to create. Determines the task's category, such as standard task or milestone.</param>
    /// <param name="status">The initial status to assign to the new task.</param>
    /// <param name="priority">The priority level of the task, indicating its relative importance.</param>
    /// <param name="progress">The current progress of the task, or null if not specified.</param>
    /// <param name="parentId">The unique identifier of the parent task, or null if the new task is a top-level task. If specified, the parent
    /// task must exist and cannot be a milestone.</param>
    /// <param name="plannedDateRange">The planned date range for the task, or null if not specified.</param>
    /// <param name="plannedDate">The planned date for the task, or null if not specified.</param>
    /// <param name="estimatedEffortHours">The estimated effort required to complete the task, in hours. Can be null if not specified.</param>
    /// <param name="assignments">A mapping of task roles to sets of user identifiers assigned to each role, or null if no assignments are
    /// specified.</param>
    /// <returns>A result containing the newly created project task if successful; otherwise, a failure result with an error
    /// message.</returns>
    public Result<ProjectTask> CreateTask(
        int nextNumber,
        string name,
        string? description,
        ProjectTaskType type,
        Enums.TaskStatus status,
        TaskPriority priority,
        Progress? progress,
        Guid? parentId,
        FlexibleDateRange? plannedDateRange,
        LocalDate? plannedDate,
        decimal? estimatedEffortHours,
        Dictionary<TaskRole, HashSet<Guid>>? assignments)
    {
        if (Key is null)
        {
            return Result.Failure<ProjectTask>("Project must have a key before tasks can be created.");
        }

        ProjectTask? parent = null;
        if (parentId.HasValue)
        {
            parent = _tasks.FirstOrDefault(t => t.Id == parentId);
            if (parent is null)
            {
                return Result.Failure<ProjectTask>("Parent task not found.");
            }

            if (parent.Type == ProjectTaskType.Milestone)
            {
                return Result.Failure<ProjectTask>("Milestones cannot have child tasks.");
            }
        }

        // Calculate order
        var siblings = parentId.HasValue
            ? _tasks.Where(t => t.ParentId == parentId)
            : _tasks.Where(t => t.ParentId is null);
        var order = siblings.Any() ? siblings.Max(t => t.Order) + 1 : 1;

        var task = ProjectTask.Create(
            Id,
            new ProjectTaskKey(Key, nextNumber),
            name,
            description,
            type,
            status,
            priority,
            progress,
            order,
            parentId,
            plannedDateRange,
            plannedDate,
            estimatedEffortHours,
            assignments);

        _tasks.Add(task);

        parent?.AddChild(task);

        return Result.Success(task);
    }

    /// <summary>
    /// Changes the parent of the specified task to a new parent task or to the root level.
    /// </summary>
    /// <remarks>If the new parent is a milestone, the operation fails because milestones cannot have child
    /// tasks. The order of the task within its new parent is set to follow existing children, and the order of siblings
    /// in the original parent is updated to maintain sequence.</remarks>
    /// <param name="taskId">The unique identifier of the task to update.</param>
    /// <param name="newParentId">The unique identifier of the new parent task. If null, the task is moved to the root level.</param>
    /// <returns>A Result indicating whether the operation succeeded. Returns a failure result if the task or new parent is not
    /// found, or if the new parent is a milestone.</returns>
    public Result ChangeTaskParent(Guid taskId, Guid? newParentId)
    {
        // get the current task
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null)
        {
            return Result.Failure("Task not found.");
        }

        var origParentId = task.ParentId;

        // get the new parent task if specified
        ProjectTask? newParent = null;
        if (newParentId.HasValue)
        {
            newParent = _tasks.FirstOrDefault(t => t.Id == newParentId);
            if (newParent is null)
            {
                return Result.Failure("New parent task not found.");
            }

            if (newParent.Type == ProjectTaskType.Milestone)
            {
                return Result.Failure("Milestones cannot have child tasks.");
            }
        }

        // set order in new parent
        var parentChildren = newParentId.HasValue
            ? _tasks.Count(t => t.ParentId == newParentId)
            : _tasks.Count(t => t.ParentId is null);

        var changeResult = task.ChangeParent(newParentId, parentChildren + 1);
        if (changeResult.IsFailure)
        {
            return changeResult;
        }

        BalanceOrderForChildTasks(origParentId);

        return Result.Success();
    }


    /// <summary>
    /// Deletes the task with the specified identifier if it has no child tasks or active dependencies.
    /// </summary>
    /// <remarks>A task cannot be deleted if it has child tasks or if it has active dependencies. Remove all
    /// child tasks and dependencies before attempting to delete the task.</remarks>
    /// <param name="taskId">The unique identifier of the task to delete.</param>
    /// <returns>A Result indicating whether the deletion was successful. Returns a failure result if the task does not exist,
    /// has child tasks, or has active dependencies.</returns>
    public Result DeleteTask(Guid taskId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null)
        {
            return Result.Failure("Task not found.");
        }

        // Check if task has children
        if (task.Children.Count > 0)
        {
            return Result.Failure("Cannot delete a task with children. Delete child tasks first.");
        }

        // Check if task has active dependencies
        if (task.Successors.Any(d => d.IsActive) || task.Predecessors.Any(d => d.IsActive))
        {
            return Result.Failure("Cannot delete a task with active dependencies. Remove dependencies first.");
        }

        var parentId = task.ParentId;

        _tasks.Remove(task);

        BalanceOrderForChildTasks(parentId);

        return Result.Success();
    }

    private void BalanceOrderForChildTasks(Guid? parentId)
    {
        var siblings = parentId.HasValue
            ? _tasks.Where(t => t.ParentId == parentId.Value)
            : _tasks.Where(t => t.ParentId is null);
        if (siblings.Any())
        {
            int order = 1;
            foreach (var sibling in siblings.OrderBy(t => t.Order))
            {
                sibling.SetOrder(order);
                order++;
            }
        }
    }

    #endregion Tasks

    /// <summary>
    /// Creates a new project.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="key"></param>
    /// <param name="expenditureCategoryId"></param>
    /// <param name="dateRange"></param>
    /// <param name="portfolioId"></param>
    /// <param name="programId"></param>
    /// <param name="roles"></param>
    /// <param name="strategicThemes"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    internal static Project Create(string name, string description, ProjectKey key, int expenditureCategoryId, LocalDateRange? dateRange, Guid portfolioId, Guid? programId, Dictionary<ProjectRole, HashSet<Guid>>? roles, HashSet<Guid>? strategicThemes, Instant timestamp)
    {
        var project = new Project(name, description, key, ProjectStatus.Proposed, expenditureCategoryId, dateRange, portfolioId, programId, roles, strategicThemes);

        project.AddPostPersistenceAction(() => project.AddDomainEvent(
            new ProjectCreatedEvent
            (
                project, 
                project.ExpenditureCategoryId, 
                (int)project.Status, 
                project.DateRange, 
                project.PortfolioId, 
                project.ProgramId,
                project.Roles
                    .GroupBy(x => (int)x.Role)
                    .ToDictionary(x => x.Key, x => x.Select(y => y.EmployeeId).ToArray()),
                [.. project.StrategicThemeTags.Select(x => x.StrategicThemeId)],
                timestamp
            )));

        return project;
    }

    /// <summary>
    /// Updates the project's key and cascades the change to all linked tasks by updating their task keys.
    /// </summary>
    public Result ChangeKey(ProjectKey key)
    {
        Guard.Against.Null(key, nameof(key));

        if (Key.Value == key.Value)
        {
            return Result.Success();
        }

        Key = key;

        foreach (var task in _tasks)
        {
            var updateResult = task.UpdateProjectKey(Key);
            if (updateResult.IsFailure)
            {
                return updateResult;
            }
        }

        return Result.Success();
    }
}
