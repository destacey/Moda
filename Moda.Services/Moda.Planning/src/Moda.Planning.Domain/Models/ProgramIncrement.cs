using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Organization.Domain.Enums;
using Moda.Planning.Domain.Enums;
using NodaTime;

namespace Moda.Planning.Domain.Models;
public class ProgramIncrement : BaseAuditableEntity<Guid>
{
    private string _name = default!;
    private string? _description;
    private LocalDateRange _dateRange = default!;

    protected readonly List<ProgramIncrementTeam> _teams = new();
    protected readonly List<ProgramIncrementObjective> _objectives = new();

    private ProgramIncrement() { }

    private ProgramIncrement(string name, string? description, LocalDateRange dateRange)
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
    /// The name of the Program Increment.
    /// </summary>
    public string Name
    {
        get => _name;
        protected set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>
    /// The description of the Program Increment.
    /// </summary>
    public string? Description
    {
        get => _description;
        protected set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>Gets or sets the date range.</summary>
    /// <value>The date range.</value>
    public LocalDateRange DateRange
    {
        get => _dateRange;
        protected set => _dateRange = Guard.Against.Null(value, nameof(DateRange));
    }

    public bool ObjectivesLocked { get; private set; } = false;

    /// <summary>Gets the teams.</summary>
    /// <value>The PI teams.</value>
    public IReadOnlyCollection<ProgramIncrementTeam> Teams => _teams.AsReadOnly();

    /// <summary>Gets the objectives.</summary>
    /// <value>The PI objectives.</value>
    public IReadOnlyCollection<ProgramIncrementObjective> Objectives => _objectives.AsReadOnly();

    /// <summary>Updates the specified name.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="dateRange">The date range.</param>
    /// <param name="objectivesLocked">if set to <c>true</c> [objectives locked].</param>
    /// <returns></returns>
    public Result Update(string name, string? description, LocalDateRange dateRange, bool objectivesLocked)
    {
        try
        {
            Name = name;
            Description = description;
            DateRange = dateRange;
            ObjectivesLocked = objectivesLocked;

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    /// <summary>States the on.</summary>
    /// <param name="date">The date.</param>
    /// <returns></returns>
    public IterationState StateOn(LocalDate date)
    {
        if (DateRange.IsPastOn(date)) { return IterationState.Completed; }
        if (DateRange.IsActiveOn(date)) { return IterationState.Active; }
        return IterationState.Future;
    }

    /// <summary>
    /// Determines whether this program increment can create objectives.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if this program increment can create objectives; otherwise, <c>false</c>.
    /// </returns>
    public bool CanCreateObjectives()
    {
        return !ObjectivesLocked;
    }

    /// <summary>Manages the program increment teams.</summary>
    /// <param name="teamIds">The team ids.</param>
    /// <returns></returns>
    public Result ManageProgramIncrementTeams(IEnumerable<Guid> teamIds)
    {
        try
        {
            var removedTeams = _teams.Where(x => !teamIds.Contains(x.TeamId)).ToList();
            foreach (var removedTeam in removedTeams)
            {
                _teams.Remove(removedTeam);
            }

            var addedTeams = teamIds.Where(x => !_teams.Any(y => y.TeamId == x)).ToList();
            foreach (var addedTeam in addedTeams)
            {
                _teams.Add(new ProgramIncrementTeam(Id, addedTeam));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

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
                return Result.Failure("Objectives are locked for this Program Increment.");

            var objectiveType = team.Type == TeamType.Team
                ? ProgramIncrementObjectiveType.Team
                : ProgramIncrementObjectiveType.TeamOfTeams;

            var objective = new ProgramIncrementObjective(Id, team.Id, objectiveId, objectiveType, isStretch);
            _objectives.Add(objective);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public Result<ProgramIncrementObjective> UpdateObjective(Guid piObjectiveId, bool isStretch)
    {
        try
        {
            var existingObjective = _objectives.FirstOrDefault(x => x.Id == piObjectiveId);
            if (existingObjective == null)
                return Result.Failure<ProgramIncrementObjective>($"Objective {piObjectiveId} not found.");

            if (ObjectivesLocked)
                isStretch = existingObjective.IsStretch;

            var updateResult = existingObjective.Update(isStretch);
            if (updateResult.IsFailure)
                return Result.Failure<ProgramIncrementObjective>(updateResult.Error);

            return Result.Success(existingObjective);
        }
        catch (Exception ex)
        {
            return Result.Failure<ProgramIncrementObjective>(ex.ToString());
        }
    }

    public Result DeleteObjective(Guid piObjectiveId)
    {
        try
        {
            if (ObjectivesLocked)
                return Result.Failure("Objectives are locked for this Program Increment.");

            var existingObjective = _objectives.FirstOrDefault(x => x.Id == piObjectiveId);
            if (existingObjective == null)
                return Result.Failure($"Program Increment Objective {piObjectiveId} not found.");

            // TODO: deleting it here is not soft deleting it
            //_objectives.Remove(existingObjective);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    /// <summary>Creates the specified name.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="dateRange">The date range.</param>
    /// <returns></returns>
    public static ProgramIncrement Create(string name, string? description, LocalDateRange dateRange)
    {
        return new ProgramIncrement(name, description, dateRange);
    }
}
