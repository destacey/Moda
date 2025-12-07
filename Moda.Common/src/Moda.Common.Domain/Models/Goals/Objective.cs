using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Data;
using Moda.Common.Domain.Enums.Goals;
using Moda.Common.Extensions;
using NodaTime;

namespace Moda.Goals.Domain.Models;

// TODO make a BaseObjective to inherit Objective and KeyResult from
public class Objective : BaseSoftDeletableEntity<Guid>, IHasIdAndKey
{
    private string _name = default!;
    private string? _description;
    private double _progress;

    private Objective() { }

    public Objective(string name, string? description, ObjectiveType type, Guid? ownerId, Guid? planId, LocalDate? startDate, LocalDate? targetDate, int? order)
    {
        Status = ObjectiveStatus.NotStarted;
        Progress = 0.0d;

        Name = name;
        Description = description;
        Type = type;
        OwnerId = ownerId;
        PlanId = planId;
        StartDate = startDate;
        TargetDate = targetDate;
        Order = order;
    }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public int Key { get; private init; }

    /// <summary>
    /// The name of the Objective.
    /// </summary>
    public string Name
    {
        get => _name;
        protected set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>
    /// The description of the Objective.
    /// </summary>
    public string? Description
    {
        get => _description;
        protected set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>Gets or sets the type.</summary>
    /// <value>The type.</value>
    public ObjectiveType Type { get; init; }

    /// <summary>Gets or sets the status.</summary>
    /// <value>The status.</value>
    public ObjectiveStatus Status { get; private set; }

    /// <summary>Gets the progress percentage.</summary>
    /// <value>The progress percentage.</value>
    public double Progress
    {
        // TODO: switch to decimal
        get => _progress;
        private set => _progress = value < 0
            ? 0.0d
            : value > 100
                ? 100.0d
                : value;
    }

    /// <summary>Gets or sets the owner identifier.</summary>
    /// <value>The owner identifier.</value>
    public Guid? OwnerId { get; private set; }

    /// <summary>Gets or sets the plan identifier.</summary>
    /// <value>The plan identifier.</value>
    public Guid? PlanId { get; private set; }

    /// <summary>Gets or sets the start date.</summary>
    /// <value>The start date.</value>
    public LocalDate? StartDate { get; private set; }

    /// <summary>Gets or sets the target date.</summary>
    /// <value>The target date.</value>
    public LocalDate? TargetDate { get; private set; }

    /// <summary>Gets the closed date.</summary>
    /// <value>The closed date.</value>
    public Instant? ClosedDate { get; private set; }

    /// <summary>
    /// The order of the Objective compared to other Objectives in the same context.
    /// </summary>
    public int? Order { get; private set; }

    /// <summary>Updates the specified objective.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="status">The status.</param>
    /// <param name="ownerId">The owner identifier.</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="targetDate">The target date.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns></returns>
    public Result Update(string name, string? description, ObjectiveStatus status, double progress, Guid? ownerId, LocalDate? startDate, LocalDate? targetDate, Instant timestamp)
    {
        try
        {
            ChangeStatus(status, timestamp);

            Name = name;
            Description = description;
            Progress = progress;
            OwnerId = ownerId;
            StartDate = startDate;
            TargetDate = targetDate;

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public void UpdateOrder(int? order)
    {
        Order = order;
    }

    private void ChangeStatus(ObjectiveStatus status, Instant timestamp)
    {
        if (Status == status) return;

        if (Status is ObjectiveStatus.Completed or ObjectiveStatus.Canceled or ObjectiveStatus.Missed
            && status is not ObjectiveStatus.Completed or ObjectiveStatus.Canceled or ObjectiveStatus.Missed)
        {
            ClosedDate = null;
        }
        else if (status is ObjectiveStatus.Completed or ObjectiveStatus.Canceled or ObjectiveStatus.Missed)
        {
            ClosedDate = timestamp;
        }

        Status = status;
    }

    /// <summary>Creates an objective.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="ownerId">The owner identifier.</param>
    /// <param name="planId">The plan identifier.</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="targetDate">The target date.</param>
    /// <param name="order">The order compared to other objectives in the same context.</param>
    /// <returns></returns>
    public static Objective Create(string name, string? description, ObjectiveType type, Guid? ownerId, Guid? planId, LocalDate? startDate, LocalDate? targetDate, int? order)
    {
        return new Objective(name, description, type, ownerId, planId, startDate, targetDate, order);
    }

    /// <summary>Creates an objective.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="type">The type.</param>
    /// <param name="status">The status.</param>
    /// <param name="ownerId">The owner identifier.</param>
    /// <param name="planId">The plan identifier.</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="targetDate">The target date.</param>
    /// <param name="closedDate">The closed date.</param>
    /// <param name="order">The order compared to other objectives in the same context.</param>
    /// <returns></returns>
    public static Objective Import(string name, string? description, ObjectiveType type, ObjectiveStatus status, double progress, Guid? ownerId, Guid? planId, LocalDate? startDate, LocalDate? targetDate, Instant? closedDate, int? order)
    {
        return new Objective()
        {
            Name = name,
            Description = description,
            Type = type,
            Status = status,
            Progress = progress,
            OwnerId = ownerId,
            PlanId = planId,
            StartDate = startDate,
            TargetDate = targetDate,
            ClosedDate = closedDate,
            Order = order
        };
    }
}
