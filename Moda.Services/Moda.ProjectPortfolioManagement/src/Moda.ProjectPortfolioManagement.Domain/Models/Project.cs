using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using NodaTime;

namespace Moda.ProjectPortfolioManagement.Domain.Models;

/// <summary>
/// Represents an individual project within a portfolio or program.
/// </summary>
public sealed class Project : BaseEntity<Guid>, ISystemAuditable, HasIdAndKey
{
    private string _name = default!;
    private string _description = default!;
    private readonly HashSet<RoleAssignment<ProjectRole>> _roles = [];

    private readonly HashSet<StrategicTheme> _strategicThemes = [];

    private Project() { }

    private Project(string name, string description, ProjectStatus status, int expenditureCategoryId, LocalDateRange? dateRange, Guid portfolioId, Guid? programId = null, Dictionary<ProjectRole, HashSet<Guid>>? roles = null)
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
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>
    /// A detailed description of the project's purpose and scope.
    /// </summary>
    public string Description
    {
        get => _description;
        private set => _description = Guard.Against.NullOrWhiteSpace(value, nameof(Description)).Trim();
    }

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
    /// The expenditure category ID.
    /// </summary>
    public int ExpenditureCategoryId { get; private set; }

    /// <summary>
    /// The expenditure category associated with the project.
    /// </summary>
    public ExpenditureCategory? ExpenditureCategory { get; private set; }

    /// <summary>
    /// The ID of the portfolio to which this project belongs.
    /// </summary>
    public Guid PortfolioId { get; private set; }

    /// <summary>
    /// The portfolio associated with this project.
    /// </summary>
    public ProjectPortfolio? Portfolio { get; private set; }

    /// <summary>
    /// The ID of the program to which this project belongs (optional).
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
    /// The strategic themes associated with this project.
    /// </summary>
    public IReadOnlyList<StrategicTheme> StrategicThemes => _strategicThemes.ToList().AsReadOnly();

    /// <summary>
    /// Updates the project details.
    /// </summary>
    public Result UpdateDetails(string name, string description, int expenditureCategoryId)
    {
        Name = name;
        Description = description;
        ExpenditureCategoryId = expenditureCategoryId;

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
            return Result.Failure("An active and completed project must have a start and end date.");
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
    public Result AddStrategicTheme(StrategicTheme strategicTheme)
    {
        Guard.Against.Null(strategicTheme, nameof(strategicTheme));

        if (_strategicThemes.Contains(strategicTheme))
        {
            return Result.Failure("This strategic theme is already associated with the project.");
        }

        _strategicThemes.Add(strategicTheme);
        return Result.Success();
    }

    /// <summary>
    /// Removes a strategic theme from this project.
    /// </summary>
    public Result RemoveStrategicTheme(StrategicTheme strategicTheme)
    {
        Guard.Against.Null(strategicTheme, nameof(strategicTheme));

        if (!_strategicThemes.Contains(strategicTheme))
        {
            return Result.Failure("This strategic theme is not associated with the project.");
        }

        _strategicThemes.Remove(strategicTheme);
        return Result.Success();
    }

    /// <summary>
    /// Updates the strategic themes associated with this project.
    /// </summary>
    /// <param name="themes"></param>
    /// <returns></returns>
    public Result UpdateStrategicThemes(IEnumerable<StrategicTheme> themes)
    {
        Guard.Against.Null(themes, nameof(themes));

        var distinctThemes = themes.DistinctBy(t => t.Id).ToList();

        // No changes needed if the themes are the same
        if (_strategicThemes.Count == distinctThemes.Count && _strategicThemes.All(distinctThemes.Contains))
        {
            return Result.Failure("No changes detected in strategic themes.");
        }

        _strategicThemes.Clear();

        foreach (var theme in distinctThemes)
        {
            _strategicThemes.Add(theme);
        }

        return Result.Success();
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
    /// <returns></returns>
    internal static Project Create(string name, string description, int expenditureCategoryId, LocalDateRange? dateRange, Guid portfolioId, Guid? programId, Dictionary<ProjectRole, HashSet<Guid>>? roles = null)
    {
        return new Project(name, description, ProjectStatus.Proposed, expenditureCategoryId, dateRange, portfolioId, programId, roles);
    }
}
