using CSharpFunctionalExtensions;
using Moda.Common.Domain.Interfaces;
using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Domain.Models;
public class PlanningIntervalObjective : BaseSoftDeletableEntity<Guid>, IHasIdAndKey
{
    private PlanningIntervalObjective() { }

    internal PlanningIntervalObjective(Guid planningIntervalId, Guid teamId, Guid objectiveId, PlanningIntervalObjectiveType type, bool isStretch)
    {
        Status = ObjectiveStatus.NotStarted;

        PlanningIntervalId = planningIntervalId;
        TeamId = teamId;
        ObjectiveId = objectiveId;
        Type = type;
        IsStretch = isStretch;
    }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public int Key { get; private init; }

    /// <summary>Gets the planning interval identifier.</summary>
    /// <value>The planning interval identifier.</value>
    public Guid PlanningIntervalId { get; private init; }

    /// <summary>Gets the team identifier.</summary>
    /// <value>The team identifier.</value>
    public Guid TeamId { get; private init; }

    /// <summary>Gets the team.</summary>
    /// <value>The team.</value>
    public PlanningTeam Team { get; private set; } = default!;

    /// <summary>Gets the objective identifier.</summary>
    /// <value>The objective identifier.</value>
    public Guid ObjectiveId { get; init; }

    /// <summary>Gets or sets the type.</summary>
    /// <value>The PI objective type.</value>
    public PlanningIntervalObjectiveType Type { get; private init; }

    /// <summary>Gets or sets the status.</summary>
    /// <value>The status.</value>
    public ObjectiveStatus Status { get; private set; }

    /// <summary>Gets a value indicating whether this instance is stretch.</summary>
    /// <value><c>true</c> if this instance is stretch; otherwise, <c>false</c>.</value>
    public bool IsStretch { get; private set; } = false;

    public SimpleHealthCheck? HealthCheck { get; private set; }

    /// <summary>Updates the specified PI objective.</summary>
    /// <param name="status">The status.</param>
    /// <param name="isStretch">if set to <c>true</c> [is stretch].</param>
    /// <returns></returns>
    internal Result Update(ObjectiveStatus status, bool isStretch)
    {
        Status = status;
        IsStretch = isStretch;

        return Result.Success();
    }

    public Result AddHealthCheck(SimpleHealthCheck healthCheck)
    {
        if (HealthCheck is not null)
            return Result.Failure("Health check already exists.");

        HealthCheck = healthCheck;

        return Result.Success();
    }

    public Result RemoveHealthCheck()
    {
        if (HealthCheck is null)
            return Result.Failure("No health check to remove.");

        HealthCheck = null;

        return Result.Success();
    }   
}
