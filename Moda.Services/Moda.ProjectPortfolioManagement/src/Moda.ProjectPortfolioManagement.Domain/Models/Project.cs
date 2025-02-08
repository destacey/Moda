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

    private readonly HashSet<StrategicTheme> _strategicThemes = [];

    private Project() { }

    private Project(string name, string description, ProjectStatus status, Guid portfolioId, Guid? programId = null, FlexibleDateRange? dateRange = null)
    {
        if (status is ProjectStatus.Active && dateRange?.Start is null)
        {
            throw new InvalidOperationException("An active project must have a start date.");
        }

        if (status is ProjectStatus.Completed or ProjectStatus.Cancelled && (dateRange?.Start is null || dateRange?.End is null))
        {
            throw new InvalidOperationException("A completed or cancelled project must have a start and end date.");
        }

        Name = name;
        Description = description;
        Status = status;
        PortfolioId = portfolioId;
        ProgramId = programId;
        DateRange = dateRange;
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
    /// The date range defining the project's lifecycle.
    /// </summary>
    public FlexibleDateRange? DateRange { get; private set; }

    /// <summary>
    /// The ID of the portfolio to which this project belongs.
    /// </summary>
    public Guid PortfolioId { get; private set; }

    /// <summary>
    /// The ID of the program to which this project belongs (optional).
    /// </summary>
    public Guid? ProgramId { get; private set; }

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
    public Result UpdateDetails(string name, string description)
    {
        Name = name;
        Description = description;

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
    /// Activates the project on the specified start date.
    /// </summary>
    public Result Activate(LocalDate startDate)
    {
        Guard.Against.Null(startDate, nameof(startDate));

        if (Status != ProjectStatus.Proposed)
        {
            return Result.Failure("Only proposed projects can be activated.");
        }

        Status = ProjectStatus.Active;
        DateRange = new FlexibleDateRange(startDate);

        return Result.Success();
    }

    /// <summary>
    /// Marks the project as completed on the specified end date.
    /// </summary>
    public Result Complete(LocalDate endDate)
    {
        Guard.Against.Null(endDate, nameof(endDate));

        if (Status != ProjectStatus.Active)
        {
            return Result.Failure("Only active projects can be completed.");
        }

        if (DateRange == null)
        {
            return Result.Failure("The project must have a start date before it can be completed.");
        }

        if (endDate < DateRange.Start)
        {
            return Result.Failure("The end date cannot be earlier than the start date.");
        }

        Status = ProjectStatus.Completed;
        DateRange = new FlexibleDateRange(DateRange.Start, endDate);

        return Result.Success();
    }

    /// <summary>
    /// Cancels the project and sets an end date.
    /// </summary>
    public Result Cancel(LocalDate endDate)
    {
        Guard.Against.Null(endDate, nameof(endDate));

        if (Status is ProjectStatus.Completed or ProjectStatus.Cancelled)
        {
            return Result.Failure("The project is already completed or cancelled.");
        }

        if (DateRange == null)
        {
            return Result.Failure("The project must have a start date before it can be cancelled.");
        }

        if (endDate < DateRange.Start)
        {
            return Result.Failure("The end date cannot be earlier than the start date.");
        }

        Status = ProjectStatus.Cancelled;
        DateRange = new FlexibleDateRange(DateRange.Start, endDate);

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
    /// Creates a new project in the proposed status.
    /// </summary>
    internal static Project Create(string name, string description, Guid portfolioId)
    {
        return new Project(name, description, ProjectStatus.Proposed, portfolioId);
    }
}
