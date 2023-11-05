using CSharpFunctionalExtensions;
using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Domain.Models;
public class ProgramIncrementObjective : BaseAuditableEntity<Guid>
{
    private ProgramIncrementObjective() { }

    internal ProgramIncrementObjective(Guid programIncrementId, Guid teamId, Guid objectiveId, ProgramIncrementObjectiveType type, bool isStretch)
    {
        Status = ObjectiveStatus.NotStarted;

        ProgramIncrementId = programIncrementId;
        TeamId = teamId;
        ObjectiveId = objectiveId;
        Type = type;
        IsStretch = isStretch;
    }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public int Key { get; init; }

    /// <summary>Gets the program increment identifier.</summary>
    /// <value>The program increment identifier.</value>
    public Guid ProgramIncrementId { get; init; }

    /// <summary>Gets the team identifier.</summary>
    /// <value>The team identifier.</value>
    public Guid TeamId { get; init; }

    /// <summary>Gets the team.</summary>
    /// <value>The team.</value>
    public PlanningTeam Team { get; private set; } = default!;

    /// <summary>Gets the objective identifier.</summary>
    /// <value>The objective identifier.</value>
    public Guid ObjectiveId { get; init; }

    /// <summary>Gets or sets the type.</summary>
    /// <value>The PI objective type.</value>
    public ProgramIncrementObjectiveType Type { get; init; }

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
    public Result Update(ObjectiveStatus status, bool isStretch)
    {
        Status = status;
        IsStretch = isStretch;

        return Result.Success();
    }
}
