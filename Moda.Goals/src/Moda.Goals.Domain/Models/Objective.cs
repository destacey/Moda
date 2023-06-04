using CSharpFunctionalExtensions;
using Moda.Goals.Domain.Enums;

namespace Moda.Goals.Domain.Models;
public class Objective : BaseAuditableEntity<Guid>
{
    private string _name = default!;
    private string? _description;

    private Objective() { }

    public Objective(string name, string? description, Guid? ownerId, Guid? planId, LocalDate? startDate, LocalDate? targetDate)
    {
        Status = ObjectiveStatus.NotStarted;

        Name = name;
        Description = description;
        OwnerId = ownerId;
        PlanId = planId;
        StartDate = startDate;
        TargetDate = targetDate;
    }

    /// <summary>Gets the local identifier.</summary>
    /// <value>The local identifier.</value>
    public int LocalId { get; private set; }

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

    /// <summary>Gets or sets the status.</summary>
    /// <value>The status.</value>
    public ObjectiveStatus Status { get; private set; }

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

    public Instant? ClosedDate { get; private set; }

    /// <summary>Updates the specified objective.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="status">The status.</param>
    /// <param name="ownerId">The owner identifier.</param>
    /// <param name="planId">The plan identifier.</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="targetDate">The target date.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns></returns>
    public Result Update(string name, string? description, ObjectiveStatus status, Guid? ownerId, Guid? planId, LocalDate? startDate, LocalDate? targetDate, Instant timestamp)
    {
        try
        {
            ChangeStatus(status, timestamp);

            Name = name;
            Description = description;
            OwnerId = ownerId;
            PlanId = planId;
            StartDate = startDate;
            TargetDate = targetDate;

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    private void ChangeStatus(ObjectiveStatus status, Instant timestamp)
    {
        if (Status == status) return;
        
        ClosedDate = status is ObjectiveStatus.Completed or ObjectiveStatus.Canceled ? timestamp : null;

        Status = status;
    }

    /// <summary>Creates an objective.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="ownerId">The owner identifier.</param>
    /// <param name="planId">The plan identifier.</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="targetDate">The target date.</param>
    /// <returns></returns>
    public static Objective Create(string name, string? description, Guid? ownerId, Guid? planId, LocalDate? startDate, LocalDate? targetDate)
    {
        return new Objective(name, description, ownerId, planId, startDate, targetDate);
    }
}
