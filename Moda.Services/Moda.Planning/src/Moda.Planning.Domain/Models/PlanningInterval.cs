﻿using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Organization.Domain.Enums;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Interfaces;
using NodaTime;

namespace Moda.Planning.Domain.Models;
public class PlanningInterval : BaseAuditableEntity<Guid>, ILocalSchedule
{
    private string _name = default!;
    private string? _description;
    private LocalDateRange _dateRange = default!;

    private readonly List<PlanningIntervalTeam> _teams = new();
    private readonly List<PlanningIntervalIteration> _iterations = new();
    private readonly List<PlanningIntervalObjective> _objectives = new();

    private PlanningInterval() { }

    private PlanningInterval(string name, string? description, LocalDateRange dateRange)
    {
        Name = name;
        Description = description;
        DateRange = dateRange;

        ObjectivesLocked = false;
    }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public int Key { get; private set; }

    /// <summary>
    /// The name of the Planning Interval.
    /// </summary>
    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>
    /// The description of the Planning Interval.
    /// </summary>
    public string? Description
    {
        get => _description;
        private set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>Gets or sets the date range.</summary>
    /// <value>The date range.</value>
    public LocalDateRange DateRange
    {
        get => _dateRange;
        private set => _dateRange = Guard.Against.Null(value, nameof(DateRange));
    }

    public bool ObjectivesLocked { get; private set; } = false;

    /// <summary>Gets the teams.</summary>
    /// <value>The PI teams.</value>
    public IReadOnlyCollection<PlanningIntervalTeam> Teams => _teams.AsReadOnly();

    /// <summary>Gets the iterations.</summary>
    /// <value>The PI iterations.</value>
    public IReadOnlyCollection<PlanningIntervalIteration> Iterations => _iterations.AsReadOnly();

    /// <summary>Gets the objectives.</summary>
    /// <value>The PI objectives.</value>
    public IReadOnlyCollection<PlanningIntervalObjective> Objectives => _objectives.AsReadOnly();

    public double? CalculatePredictability(LocalDate date, Guid? teamId = null)
    {
        if (StateOn(date) == IterationState.Future)
            return null;

        var objectives = _objectives.Where(o => o.Type == PlanningIntervalObjectiveType.Team);
        if (teamId.HasValue)
            objectives = _objectives.Where(o => o.TeamId == teamId.Value).ToList();

        if (!objectives.Any())
            return null;

        var nonstretchCount = objectives.Count(o => !o.IsStretch);
        if (nonstretchCount == 0) { return 0; }

        var completedCount = objectives.Count(o => o.Status == ObjectiveStatus.Completed);
        return completedCount >= nonstretchCount ? 100.0d : Math.Round(100 * ((double)completedCount / nonstretchCount), 2);
    }

    /// <summary>Updates the specified name.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="objectivesLocked">if set to <c>true</c> [objectives locked].</param>
    /// <returns></returns>
    public Result Update(string name, string? description, bool objectivesLocked)
    {
        Name = name;
        Description = description;
        ObjectivesLocked = objectivesLocked;

        return Result.Success();
    }

    /// <summary>Iteration state on given date.</summary>
    /// <param name="date">The date.</param>
    /// <returns></returns>
    public IterationState StateOn(LocalDate date)
    {
        if (DateRange.IsPastOn(date)) { return IterationState.Completed; }
        if (DateRange.IsActiveOn(date)) { return IterationState.Active; }
        return IterationState.Future;
    }

    /// <summary>
    /// Determines whether this planning interval can create objectives.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if this planning interval can create objectives; otherwise, <c>false</c>.
    /// </returns>
    public bool CanCreateObjectives()
    {
        return !ObjectivesLocked;
    }

    /// <summary>Manages the planning interval teams.</summary>
    /// <param name="teamIds">The team ids.</param>
    /// <returns></returns>
    public Result ManageTeams(IEnumerable<Guid> teamIds)
    {
        var removedTeams = _teams.Where(x => !teamIds.Contains(x.TeamId)).ToList();
        foreach (var removedTeam in removedTeams)
        {
            _teams.Remove(removedTeam);
        }

        var addedTeams = teamIds.Where(x => !_teams.Any(y => y.TeamId == x)).ToList();
        foreach (var addedTeam in addedTeams)
        {
            _teams.Add(new PlanningIntervalTeam(Id, addedTeam));
        }

        return Result.Success();
    }

    public Result ManageDates(PlanningIntervalCalendar calendar)
    {
        if (Id != calendar.Id)
            return Result.Failure("Planning Interval Id does not match Id in the Calendar.");

        DateRange = calendar.DateRange;

        return Result.Success();
    }

    #region Iterations

    //public Result AddIteration(string name, IterationType type, LocalDateRange dateRange)
    //{
    //    if (Iterations.Any(x => x.Name == name))
    //        return Result.Failure("Iteration name already exists.");

    //    if (Iterations.Any(x => x.DateRange.Overlaps(dateRange)))
    //        return Result.Failure("Iteration date range overlaps with existing iteration date range.");

    //    if (dateRange.Start < DateRange.Start)
    //        return Result.Failure("Iteration date range cannot start before the Planning Interval date range.");

    //    if (dateRange.End > DateRange.End)
    //        return Result.Failure("Iteration date range cannot end after the Planning Interval date range.");

    //    var iteration = new PlanningIntervalIteration(Id, name, type, dateRange);
    //    _iterations.Add(iteration);

    //    return Result.Success();
    //}

    public Result UpdateIteration(Guid iterationId, string name, IterationType type)
    {
        var existingIteration = _iterations.FirstOrDefault(x => x.Id == iterationId);
        if (existingIteration == null)
            return Result.Failure($"Iteration {iterationId} not found.");

        if (existingIteration.Name != name && _iterations.Any(x => x.Name == name))
            return Result.Failure("Iteration name already exists.");

        //if (existingIteration.DateRange != dateRange && _iterations.Any(x => x.DateRange.Overlaps(dateRange)))
        //    return Result.Failure("Iteration date range overlaps with existing iteration date range.");

        //if (dateRange.Start < DateRange.Start)
        //    return Result.Failure("Iteration date range cannot start before the Planning Interval date range.");

        //if (dateRange.End > DateRange.End)
        //    return Result.Failure("Iteration date range cannot end after the Planning Interval date range.");

        var updateResult = existingIteration.Update(name, type);
        return updateResult.IsSuccess ? Result.Success() : Result.Failure(updateResult.Error);
    }

    public Result DeleteIteration(Guid iterationId)
    {
        var existingIteration = _iterations.FirstOrDefault(x => x.Id == iterationId);
        if (existingIteration == null)
            return Result.Failure($"Iteration {iterationId} not found.");

        _iterations.Remove(existingIteration);

        return Result.Success();
    }


    #endregion Iterations

    #region Objectives

    /// <summary>Creates a PI objective.</summary>
    /// <param name="team">The team.</param>
    /// <param name="objectiveId">The objective identifier.</param>
    /// <param name="isStretch">if set to <c>true</c> [is stretch].</param>
    /// <returns></returns>
    public Result CreateObjective(PlanningTeam team, Guid objectiveId, bool isStretch)
    {
        try
        {
            if (!CanCreateObjectives())
                return Result.Failure("Objectives are locked for this Planning Interval.");

            var objectiveType = team.Type == TeamType.Team
                ? PlanningIntervalObjectiveType.Team
                : PlanningIntervalObjectiveType.TeamOfTeams;

            var objective = new PlanningIntervalObjective(Id, team.Id, objectiveId, objectiveType, isStretch);
            _objectives.Add(objective);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public Result<PlanningIntervalObjective> UpdateObjective(Guid piObjectiveId, ObjectiveStatus status, bool isStretch)
    {
        try
        {
            var existingObjective = _objectives.FirstOrDefault(x => x.Id == piObjectiveId);
            if (existingObjective == null)
                return Result.Failure<PlanningIntervalObjective>($"Objective {piObjectiveId} not found.");

            if (ObjectivesLocked)
                isStretch = existingObjective.IsStretch;

            var updateResult = existingObjective.Update(status, isStretch);
            if (updateResult.IsFailure)
                return Result.Failure<PlanningIntervalObjective>(updateResult.Error);

            return Result.Success(existingObjective);
        }
        catch (Exception ex)
        {
            return Result.Failure<PlanningIntervalObjective>(ex.ToString());
        }
    }

    public Result DeleteObjective(Guid piObjectiveId)
    {
        try
        {
            if (ObjectivesLocked)
                return Result.Failure("Objectives are locked for this Planning Interval.");

            var existingObjective = _objectives.FirstOrDefault(x => x.Id == piObjectiveId);
            if (existingObjective == null)
                return Result.Failure($"Planning Interval Objective {piObjectiveId} not found.");

            // TODO: deleting it here is not soft deleting it
            //_objectives.Remove(existingObjective);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    #endregion Objectives

    /// <summary>Creates the specified name.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="dateRange">The date range.</param>
    /// <returns></returns>
    public static PlanningInterval Create(string name, string? description, LocalDateRange dateRange)
    {
        return new PlanningInterval(name, description, dateRange);
    }
}