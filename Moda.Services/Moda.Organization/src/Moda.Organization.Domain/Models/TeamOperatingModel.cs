using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Organization.Domain.Enums;
using NodaTime;

namespace Moda.Organization.Domain.Models;

/// <summary>
/// Represents the operating model for a team, defining how the team works
/// (methodology and sizing method) for a specific date range.
/// </summary>
public sealed class TeamOperatingModel : BaseEntity<Guid>, ISystemAuditable
{
    private TeamOperatingModel() { }

    private TeamOperatingModel(OperatingModelDateRange dateRange, Methodology methodology, SizingMethod sizingMethod)
    {
        DateRange = dateRange;
        Methodology = methodology;
        SizingMethod = sizingMethod;
    }

    /// <summary>Gets the effective date range for this operating model.</summary>
    public OperatingModelDateRange DateRange { get; private set; } = null!;

    /// <summary>Gets the methodology the team uses.</summary>
    public Methodology Methodology { get; private set; }

    /// <summary>Gets the sizing method the team uses.</summary>
    public SizingMethod SizingMethod { get; private set; }

    /// <summary>Gets whether this operating model is current (has no end date).</summary>
    public bool IsCurrent => DateRange.IsCurrent;

    /// <summary>
    /// Updates the methodology and sizing method for this operating model.
    /// </summary>
    /// <param name="methodology">The new methodology.</param>
    /// <param name="sizingMethod">The new sizing method.</param>
    /// <returns>A result indicating success or failure.</returns>
    public Result Update(Methodology methodology, SizingMethod sizingMethod)
    {
        Methodology = methodology;
        SizingMethod = sizingMethod;
        return Result.Success();
    }

    /// <summary>
    /// Closes this operating model by setting its end date.
    /// </summary>
    /// <param name="endDate">The end date for this operating model.</param>
    internal void Close(LocalDate endDate)
    {
        DateRange.SetEnd(endDate);
    }

    /// <summary>
    /// Clears the end date from the current date range, leaving only the start date set.
    /// </summary>
    internal void ClearEndDate()
    {
        DateRange.ClearEnd();
    }

    /// <summary>
    /// Creates a new team operating model.
    /// If a current model exists, it will be closed with an end date of one day before the new model's start date.
    /// </summary>
    /// <param name="startDate">The start date for this operating model.</param>
    /// <param name="methodology">The methodology the team uses.</param>
    /// <param name="sizingMethod">The sizing method the team uses.</param>
    /// <param name="currentModel">The current operating model, if one exists.</param>
    /// <returns>A result containing the new operating model or an error.</returns>
    internal static Result<TeamOperatingModel> Create(
        LocalDate startDate,
        Methodology methodology,
        SizingMethod sizingMethod,
        TeamOperatingModel? currentModel = null)
    {
        // If there's a current model, validate and close it
        if (currentModel is not null && currentModel.IsCurrent)
        {
            // The new model's start date must be after the current model's start date
            if (startDate <= currentModel.DateRange.Start)
            {
                return Result.Failure<TeamOperatingModel>(
                    "New operating model start date must be after the current model's start date.");
            }

            // Close the current model one day before the new one starts
            var previousEndDate = startDate.PlusDays(-1);
            currentModel.Close(previousEndDate);
        }

        var dateRange = new OperatingModelDateRange(startDate, null);
        var model = new TeamOperatingModel(dateRange, methodology, sizingMethod);

        return Result.Success(model);
    }
}
