using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using NodaTime;

namespace Moda.ProjectPortfolioManagement.Domain.Models;

/// <summary>
/// Represents a program consisting of related projects within a portfolio, designed to achieve strategic objectives.
/// </summary>
public sealed class Program : BaseEntity<Guid>, ISystemAuditable, HasIdAndKey
{
    private string _name = default!;
    private string _description = default!;

    private readonly HashSet<RoleAssignment<ProgramRole>> _roles = [];
    private readonly HashSet<Project> _projects = [];
    private readonly HashSet<StrategicThemeTag<Program>> _strategicThemeTags = [];

    private Program() { }

    private Program(string name, string description, ProgramStatus status, LocalDateRange? dateRange, Guid portfolioId, Dictionary<ProgramRole, HashSet<Guid>>? roles = null, HashSet<Guid>? strategicThemes = null)
    {
        if (Status is ProgramStatus.Active or ProgramStatus.Completed && dateRange is null)
        {
            throw new InvalidOperationException("An active and completed program must have a start and end date.");
        }

        Name = name;
        Description = description;
        Status = status;
        PortfolioId = portfolioId;
        DateRange = dateRange;

        _roles = roles?
            .SelectMany(r => r.Value
                .Select(e => new RoleAssignment<ProgramRole>(Id, r.Key, e)))
            .ToHashSet()
            ?? [];

        _strategicThemeTags = strategicThemes?.Select(t => new StrategicThemeTag<Program>(Id, t)).ToHashSet()
            ?? [];
    }

    /// <summary>
    /// The unique key of the program. This is an alternate key to the Id.
    /// </summary>
    public int Key { get; private init; }

    /// <summary>
    /// The name of the program.
    /// </summary>
    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>
    /// A detailed description of the program's purpose and scope.
    /// </summary>
    public string Description
    {
        get => _description;
        private set => _description = Guard.Against.NullOrWhiteSpace(value, nameof(Description)).Trim();
    }

    /// <summary>
    /// The current status of the program.
    /// </summary>
    public ProgramStatus Status { get; private set; }

    /// <summary>
    /// The roles associated with this program.
    /// </summary>
    public IReadOnlyCollection<RoleAssignment<ProgramRole>> Roles => _roles;

    /// <summary>
    /// The date range defining the program's lifecycle.
    /// </summary>
    public LocalDateRange? DateRange { get; private set; }

    /// <summary>
    /// The ID of the portfolio to which this program belongs.
    /// </summary>
    public Guid PortfolioId { get; private set; }

    /// <summary>
    /// The portfolio to which this program belongs.
    /// </summary>
    public ProjectPortfolio? Portfolio { get; private set; }

    /// <summary>
    /// The projects associated with this program.
    /// </summary>
    public IReadOnlyCollection<Project> Projects => _projects;

    /// <summary>
    /// Indicates if the program is currently accepting new projects.
    /// </summary>
    public bool AcceptingProjects => Status == ProgramStatus.Active;

    /// <summary>
    /// Indicates if the project is in a closed state.
    /// </summary>
    public bool IsClosed => Status is ProgramStatus.Completed or ProgramStatus.Cancelled;

    /// <summary>
    /// The strategic themes associated with this program.
    /// </summary>
    public IReadOnlyCollection<StrategicThemeTag<Program>> StrategicThemeTags => _strategicThemeTags;

    /// <summary>
    /// Indicates whether the program can be deleted.
    /// </summary>
    /// <returns></returns>
    public bool CanBeDeleted() => Status is ProgramStatus.Proposed;

    /// <summary>
    /// Updates the program details.
    /// </summary>
    public Result UpdateDetails(string name, string description)
    {
        Name = name;
        Description = description;

        return Result.Success();
    }

    public Result UpdateTimeline(LocalDateRange? dateRange)
    {
        if (Status is ProgramStatus.Active or ProgramStatus.Completed && dateRange is null)
        {
            return Result.Failure("An active and completed program must have a start and end date.");
        }

        DateRange = dateRange;

        return Result.Success();
    }

    /// <summary>
    /// Assigns an employee to a specific role within the program, allowing multiple employees per role.
    /// </summary>
    public Result AssignRole(ProgramRole role, Guid employeeId)
    {
        return RoleManager.AssignRole(_roles, Id, role, employeeId);
    }

    /// <summary>
    /// Removes an employee from a specific role.
    /// </summary>
    public Result RemoveRole(ProgramRole role, Guid employeeId)
    {
        return RoleManager.RemoveAssignment(_roles, role, employeeId);
    }

    /// <summary>
    /// Updates the roles for the program.
    /// </summary>
    /// <param name="updatedRoles"></param>
    /// <returns></returns>
    public Result UpdateRoles(Dictionary<ProgramRole, HashSet<Guid>> updatedRoles)
    {
        return RoleManager.UpdateRoles(_roles, Id, updatedRoles);
    }

    /// <summary>
    /// Associates a strategic theme with this program.
    /// </summary>
    public Result AddStrategicTheme(Guid strategicThemeId)
    {
        Guard.Against.NullOrEmpty(strategicThemeId, nameof(strategicThemeId));

        return StrategicThemeTagManager<Program>.AddStrategicThemeTag(_strategicThemeTags, Id, strategicThemeId, "program");
    }

    /// <summary>
    /// Removes a strategic theme from this program.
    /// </summary>
    public Result RemoveStrategicTheme(Guid strategicThemeId)
    {
        Guard.Against.NullOrEmpty(strategicThemeId, nameof(strategicThemeId));

        return StrategicThemeTagManager<Program>.RemoveStrategicThemeTag(_strategicThemeTags, strategicThemeId, "program");
    }

    /// <summary>
    /// Updates the strategic themes associated with this program.
    /// </summary>
    /// <param name="strategicThemeIds"></param>
    /// <returns></returns>
    public Result UpdateStrategicThemes(HashSet<Guid> strategicThemeIds)
    {
        Guard.Against.Null(strategicThemeIds, nameof(strategicThemeIds));;

        return StrategicThemeTagManager<Program>.UpdateTags(_strategicThemeTags, Id, strategicThemeIds, "program");
    }

    #region Lifecycle

    /// <summary>
    /// Activates the program.
    /// </summary>
    public Result Activate()
    {
        if (Status != ProgramStatus.Proposed)
        {
            return Result.Failure("Only proposed programs can be activated.");
        }

        if (DateRange is null)
        {
            return Result.Failure("The program must have a start and end date before it can be activated.");
        }

        Status = ProgramStatus.Active;

        return Result.Success();
    }

    /// <summary>
    /// Marks the program as completed.
    /// </summary>
    public Result Complete()
    {
        if (Status != ProgramStatus.Active)
        {
            return Result.Failure("Only active programs can be completed.");
        }

        if (DateRange is null)
        {
            return Result.Failure("The program must have a start and end date before it can be activated.");
        }

        if (_projects.Any(p => !p.IsClosed))
        {
            return Result.Failure("All projects must be completed or canceled before the program can be completed.");
        }

        Status = ProgramStatus.Completed;

        return Result.Success();
    }

    /// <summary>
    /// Cancels the program.
    /// </summary>
    public Result Cancel()
    {
        if (Status is ProgramStatus.Completed or ProgramStatus.Cancelled)
        {
            return Result.Failure("The program is already completed or cancelled.");
        }

        if (Status is ProgramStatus.Active)
        {
            if (_projects.Any(p => !p.IsClosed))
            {
                return Result.Failure("All projects must be completed or canceled before the program can be cancelled.");
            }
        }

        // Directly allow Proposed → Cancelled without setting DateRange
        Status = ProgramStatus.Cancelled;

        return Result.Success();
    }

    #endregion Lifecycle


    /// <summary>
    /// Adds an existing project to the program.
    /// </summary>
    internal Result AddProject(Project project)
    {
        Guard.Against.Null(project, nameof(project));

        if (AcceptingProjects is false)
        {
            return Result.Failure("The program is not accepting new projects.");
        }

        if (project.PortfolioId != PortfolioId)
        {
            return Result.Failure("The project must belong to the same portfolio as the program.");
        }

        if (_projects.Contains(project))
        {
            return Result.Failure("The project is already part of this program.");
        }

        var result = project.UpdateProgram(this);
        if (result.IsFailure)
        {
            return result;
        }

        _projects.Add(project);

        return Result.Success();
    }

    /// <summary>
    /// Removes an existing project from the program.
    /// </summary>
    internal Result RemoveProject(Project project)
    {
        Guard.Against.Null(project, nameof(project));

        if (!_projects.Contains(project))
        {
            return Result.Failure("The project is not part of this program.");
        }

        if (IsClosed)
        {
            return Result.Failure("Projects cannot be removed from a closed program.");
        }

        var result = project.UpdateProgram(null);
        if (result.IsFailure)
        {
            return result;
        }

        _projects.Remove(project);

        return Result.Success();
    }

    /// <summary>
    /// Checks if the program is active on the specified date.
    /// </summary>
    public bool IsActiveOn(LocalDate date)
    {
        Guard.Against.Null(date, nameof(date));

        return DateRange is not null && DateRange.IsActiveOn(date);
    }

    /// <summary>
    /// Creates a new program with the specified details.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="dateRange"></param>
    /// <param name="portfolioId"></param>
    /// <param name="roles"></param>
    /// <param name="strategicThemes"></param>
    /// <returns></returns>
    internal static Program Create(string name, string description, LocalDateRange? dateRange, Guid portfolioId, Dictionary<ProgramRole, HashSet<Guid>>? roles = null, HashSet<Guid>? strategicThemes = null)
    {
        return new Program(name, description, ProgramStatus.Proposed, dateRange, portfolioId, roles, strategicThemes);
    }
}
