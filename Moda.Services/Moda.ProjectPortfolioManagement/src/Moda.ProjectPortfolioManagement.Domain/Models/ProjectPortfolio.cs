using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using NodaTime;

namespace Moda.ProjectPortfolioManagement.Domain.Models;

/// <summary>
/// Represents a collection of projects or programs that are managed together to achieve strategic results.
/// </summary>
public sealed class ProjectPortfolio : BaseEntity<Guid>, ISystemAuditable, HasIdAndKey
{
    private string _name = default!;
    private string _description = default!;

    private readonly HashSet<Program> _programs = [];
    private readonly HashSet<Project> _projects = [];

    private ProjectPortfolio() { }

    private ProjectPortfolio(string name, string description, ProjectPortfolioStatus status, FlexibleDateRange? dateRange = null)
    {
        if (status is ProjectPortfolioStatus.Active or ProjectPortfolioStatus.OnHold && dateRange?.Start is null)
        {
            throw new InvalidOperationException("An active or on hold portfolio must have a start date.");
        }

        if (status is ProjectPortfolioStatus.Completed or ProjectPortfolioStatus.Archived && (dateRange?.Start is null || dateRange?.End is null))
        {
            throw new InvalidOperationException("A completed or archived portfolio must have a start and end date.");
        }

        Name = name;
        Description = description;
        Status = status;
        DateRange = dateRange;
    }

    /// <summary>
    /// The unique key of the portfolio.  This is an alternate key to the Id.
    /// </summary>
    public int Key { get; private init; }

    /// <summary>
    /// The name of the portfolio.
    /// </summary>
    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>
    /// A detailed description of the portfolio’s purpose.
    /// </summary>
    public string Description
    {
        get => _description;
        private set => _description = Guard.Against.NullOrWhiteSpace(value, nameof(Description)).Trim();
    }

    /// <summary>
    /// The status of the portfolio.
    /// </summary>
    public ProjectPortfolioStatus Status { get; private set; }

    /// <summary>
    /// The date range defining the portfolio’s lifecycle.
    /// </summary>
    public FlexibleDateRange? DateRange { get; private set; }

    /// <summary>
    /// The programs associated with this portfolio.
    /// </summary>
    public IReadOnlyCollection<Program> Programs => _programs;

    /// <summary>
    /// The projects associated with this portfolio.
    /// </summary>
    public IReadOnlyCollection<Project> Projects => _projects;

    /// <summary>
    /// Updates the portfolio details, including the date range.
    /// </summary>
    public Result UpdateDetails(string name, string description)
    {
        Name = name;
        Description = description;

        return Result.Success();
    }

    #region Lifecycle

    /// <summary>
    /// Activates the portfolio on the specified start date.
    /// </summary>
    /// <param name="startDate"></param>
    /// <returns></returns>
    public Result Activate(LocalDate startDate)
    {
        Guard.Against.Null(startDate, nameof(startDate));

        if (Status != ProjectPortfolioStatus.Proposed)
        {
            return Result.Failure("Only proposed portfolios can be activated.");
        }

        Status = ProjectPortfolioStatus.Active;
        DateRange = new FlexibleDateRange(startDate);

        return Result.Success();
    }

    /// <summary>
    /// Puts the portfolio on hold.
    /// </summary>
    public Result Pause()
    {
        if (Status != ProjectPortfolioStatus.Active)
        {
            return Result.Failure("Only active portfolios can be put on hold.");
        }

        Status = ProjectPortfolioStatus.OnHold;

        return Result.Success();
    }

    /// <summary>
    /// Resumes an on-hold portfolio.
    /// </summary>
    public Result Resume()
    {
        if (Status != ProjectPortfolioStatus.OnHold)
        {
            return Result.Failure("Only portfolios on hold can be resumed.");
        }

        Status = ProjectPortfolioStatus.Active;

        return Result.Success();
    }

    /// <summary>
    /// Marks the portfolio as completed.
    /// Ensures all projects or programs are resolved before transitioning to this status.
    /// </summary>
    public Result Complete(LocalDate endDate)
    {
        Guard.Against.Null(endDate, nameof(endDate));

        if (Status != ProjectPortfolioStatus.Active)
        {
            return Result.Failure("Only active portfolios can be completed.");
        }

        if (DateRange == null)
        {
            return Result.Failure("The portfolio must have a start date before it can be completed.");
        }

        if (endDate < DateRange.Start)
        {
            return Result.Failure("The end date cannot be earlier than the start date.");
        }

        if (_programs.Any(p => !p.IsClosed))
        {
            return Result.Failure("All programs must be completed or canceled before the portfolio can be completed.");
        }

        if (_projects.Any(p => !p.IsClosed))
        {
            return Result.Failure("All projects must be completed or canceled before the portfolio can be completed.");
        }

        Status = ProjectPortfolioStatus.Completed;
        DateRange = new FlexibleDateRange(DateRange.Start, endDate);

        return Result.Success();
    }

    /// <summary>
    /// Archives a completed portfolio.
    /// </summary>
    public Result Archive()
    {
        if (Status != ProjectPortfolioStatus.Completed)
        {
            return Result.Failure("Only completed portfolios can be archived.");
        }

        Status = ProjectPortfolioStatus.Archived;

        return Result.Success();
    }

    #endregion Lifecycle

    /// <summary>
    /// Creates and adds a new program to the portfolio.
    /// </summary>
    /// <param name="name">The name of the program.</param>
    /// <param name="description">The description of the program.</param>
    /// <returns>A result containing the created program or an error.</returns>
    public Result<Program> CreateProgram(string name, string description)
    {
        if (Status != ProjectPortfolioStatus.Active && Status != ProjectPortfolioStatus.OnHold)
        {
            return Result.Failure<Program>("Programs can only be created in active or on-hold portfolios.");
        }

        var program = Program.Create(name, description, Id);
        _programs.Add(program);

        return Result.Success(program);
    }


    /// <summary>
    /// Creates and adds a new project to the portfolio, optionally associating it with a valid and accepting program.
    /// </summary>
    /// <param name="name">The name of the project.</param>
    /// <param name="description">The description of the project.</param>
    /// <param name="programId">The ID of the program the project should be associated with (optional).</param>
    /// <returns>A result containing the created project or an error.</returns>
    public Result<Project> CreateProject(string name, string description, Guid? programId = null)
    {
        if (Status != ProjectPortfolioStatus.Active && Status != ProjectPortfolioStatus.OnHold)
        {
            return Result.Failure<Project>("Projects can only be created in active or on-hold portfolios.");
        }

        // Validate the program ID if provided
        Program? program = null;
        if (programId.HasValue)
        {
            program = _programs.SingleOrDefault(p => p.Id == programId.Value);
            if (program is null)
            {
                return Result.Failure<Project>("The specified program does not belong to this portfolio.");
            }

            if (program.AcceptingProjects is false)
            {
                return Result.Failure<Project>("The specified program is not in a valid state to accept projects.");
            }
        }

        // Create the project
        var project = Project.Create(name, description, Id);

        // Add the project to the portfolio's project list
        _projects.Add(project);

        // Associate the project with the program if provided
        if (program is not null)
        {
            var addToProgramResult = program.AddProject(project);
            if (addToProgramResult.IsFailure)
            {
                return Result.Failure<Project>(addToProgramResult.Error);
            }
        }

        return Result.Success(project);
    }

    public Result ChangeProjectProgram(Guid projectId, Guid? programId)
    {
        var project = _projects.SingleOrDefault(p => p.Id == projectId);
        if (project is null)
        {
            return Result.Failure("The specified project does not belong to this portfolio.");
        }

        if (project.ProgramId == programId)
        {
            return Result.Failure(programId is null
                ? "The project is not currently assigned to a program."
                : "The project is already associated with the specified program.");
        }

        var program = programId.HasValue ? _programs.SingleOrDefault(p => p.Id == programId.Value) : null;
        if (program is null && programId.HasValue)
        {
            return Result.Failure("The specified program does not belong to this portfolio.");
        }

        if (project.ProgramId.HasValue)
        {
            // remove the project from the current program
            var currentProgram = _programs.SingleOrDefault(p => p.Id == project.ProgramId.Value);
            if (currentProgram is null)
            {
                return Result.Failure("The project is associated with an invalid program.");
            }
            var removeProjectResult = currentProgram.RemoveProject(project);
            if (removeProjectResult.IsFailure)
            {
                return Result.Failure(removeProjectResult.Error);
            }
        }

        if (program is not null)
        {
            var addToProgramResult = program.AddProject(project);
            if (addToProgramResult.IsFailure)
            {
                return Result.Failure(addToProgramResult.Error);
            }
        }
        else
        {
            var removeFromProgramResult = project.UpdateProgram(null);
            if (removeFromProgramResult.IsFailure)
            {
                return Result.Failure(removeFromProgramResult.Error);
            }
        }

        return Result.Success();
    }

    /// <summary>
    /// Checks if the portfolio is active on the specified date.
    /// </summary>
    public bool IsActiveOn(LocalDate date)
    {
        Guard.Against.Null(date, nameof(date));

        return DateRange is not null && DateRange.IsActiveOn(date);
    }

    /// <summary>
    /// Creates a new portfolio in the proposed status.
    /// </summary>
    public static ProjectPortfolio Create(string name, string description)
    {
        return new ProjectPortfolio(name, description, ProjectPortfolioStatus.Proposed);
    }
}
