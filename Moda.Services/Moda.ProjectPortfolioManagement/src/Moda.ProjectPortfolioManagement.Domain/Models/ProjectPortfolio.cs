using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using NodaTime;

namespace Moda.ProjectPortfolioManagement.Domain.Models;
public sealed class ProjectPortfolio : BaseEntity<Guid>, ISystemAuditable, HasIdAndKey
{
    private string _name = default!;
    private string _description = default!;

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

        // TODO: Ensure all projects are completed or canceled before completing the portfolio.

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
