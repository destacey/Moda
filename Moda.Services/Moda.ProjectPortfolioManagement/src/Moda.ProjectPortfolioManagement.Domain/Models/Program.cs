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

    private readonly List<Project> _projects = [];

    private Program() { }

    private Program(string name, string description, ProgramStatus status, Guid portfolioId, FlexibleDateRange? dateRange = null)
    {
        if (status is ProgramStatus.Active && dateRange?.Start is null)
        {
            throw new InvalidOperationException("An active program must have a start date.");
        }

        if (status is ProgramStatus.Completed or ProgramStatus.Cancelled && (dateRange?.Start is null || dateRange?.End is null))
        {
            throw new InvalidOperationException("A completed or cancelled program must have a start and end date.");
        }

        Name = name;
        Description = description;
        Status = status;
        PortfolioId = portfolioId;
        DateRange = dateRange;
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
    /// The date range defining the program's lifecycle.
    /// </summary>
    public FlexibleDateRange? DateRange { get; private set; }

    /// <summary>
    /// The ID of the portfolio to which this program belongs.
    /// </summary>
    public Guid PortfolioId { get; private set; }

    /// <summary>
    /// The projects associated with this program.
    /// </summary>
    public IReadOnlyCollection<Project> Projects => _projects.AsReadOnly();

    /// <summary>
    /// Indicates if the program is currently accepting new projects.
    /// </summary>
    public bool AcceptingProjects => Status == ProgramStatus.Active;

    /// <summary>
    /// Indicates if the project is in a closed state.
    /// </summary>
    public bool IsClosed => Status is ProgramStatus.Completed or ProgramStatus.Cancelled;

    /// <summary>
    /// Updates the program details.
    /// </summary>
    public Result UpdateDetails(string name, string description)
    {
        Name = name;
        Description = description;

        return Result.Success();
    }

    #region Lifecycle

    /// <summary>
    /// Activates the program on the specified start date.
    /// </summary>
    public Result Activate(LocalDate startDate)
    {
        Guard.Against.Null(startDate, nameof(startDate));

        if (Status != ProgramStatus.Proposed)
        {
            return Result.Failure("Only proposed programs can be activated.");
        }

        Status = ProgramStatus.Active;
        DateRange = new FlexibleDateRange(startDate);

        return Result.Success();
    }

    /// <summary>
    /// Marks the program as completed on the specified end date.
    /// </summary>
    public Result Complete(LocalDate endDate)
    {
        Guard.Against.Null(endDate, nameof(endDate));

        if (Status != ProgramStatus.Active)
        {
            return Result.Failure("Only active programs can be completed.");
        }

        if (DateRange is null)
        {
            return Result.Failure("The program must have a start date before it can be completed.");
        }

        if (endDate < DateRange.Start)
        {
            return Result.Failure("The end date cannot be earlier than the start date.");
        }

        if (_projects.Any(p => !p.IsClosed))
        {
            return Result.Failure("All projects must be completed or canceled before the program can be completed.");
        }

        Status = ProgramStatus.Completed;
        DateRange = new FlexibleDateRange(DateRange.Start, endDate);

        return Result.Success();
    }

    /// <summary>
    /// Cancels the program and sets an end date if it was active.
    /// </summary>
    public Result Cancel(LocalDate endDate)
    {
        Guard.Against.Null(endDate, nameof(endDate));

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

            if (DateRange is null || endDate < DateRange.Start)
            {
                return Result.Failure("The end date cannot be earlier than the start date.");
            }

            DateRange = new FlexibleDateRange(DateRange.Start, endDate);
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
    /// Creates a new program in the proposed status.
    /// </summary>
    internal static Program Create(string name, string description, Guid portfolioId)
    {
        return new Program(name, description, ProgramStatus.Proposed, portfolioId);
    }
}
