using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Events.ProjectPortfolioManagement;
using Moda.Common.Domain.Interfaces.ProjectPortfolioManagement;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;
using NodaTime;

namespace Moda.ProjectPortfolioManagement.Domain.Models;

/// <summary>
/// Represents an individual project within a portfolio or program.
/// </summary>
public sealed class Project : BaseEntity<Guid>, ISystemAuditable, IHasIdAndKey, ISimpleProject
{
    private readonly HashSet<RoleAssignment<ProjectRole>> _roles = [];
    private readonly HashSet<StrategicThemeTag<Project>> _strategicThemeTags = [];
    private readonly HashSet<StrategicInitiativeProject> _strategicInitiativeProjects = [];

    private Project() { }

    private Project(string name, string description, ProjectStatus status, int expenditureCategoryId, LocalDateRange? dateRange, Guid portfolioId, Guid? programId = null, Dictionary<ProjectRole, HashSet<Guid>>? roles = null, HashSet<Guid>? strategicThemes = null)
    {
        if (Status is ProjectStatus.Active or ProjectStatus.Completed && dateRange is null)
        {
            throw new InvalidOperationException("An active and completed project must have a start and end date.");
        }

        Name = name;
        Description = description;
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
    /// The unique key of the project. This is an alternate key to the Id.
    /// </summary>
    public int Key { get; private init; }

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

    /// <summary>
    /// Creates a new project.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="expenditureCategoryId"></param>
    /// <param name="dateRange"></param>
    /// <param name="portfolioId"></param>
    /// <param name="programId"></param>
    /// <param name="roles"></param>
    /// <param name="strategicThemes"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    internal static Project Create(string name, string description, int expenditureCategoryId, LocalDateRange? dateRange, Guid portfolioId, Guid? programId, Dictionary<ProjectRole, HashSet<Guid>>? roles, HashSet<Guid>? strategicThemes, Instant timestamp)
    {
        var project = new Project(name, description, ProjectStatus.Proposed, expenditureCategoryId, dateRange, portfolioId, programId, roles, strategicThemes);

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
}
