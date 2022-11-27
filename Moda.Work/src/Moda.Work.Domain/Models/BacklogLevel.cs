using CSharpFunctionalExtensions;
using NodaTime;

namespace Moda.Work.Domain.Models;

/// <summary>
/// A backlog level helps abstract work types
/// </summary>
public sealed class BacklogLevel : BaseAuditableEntity<Guid>, IActivatable
{
    private BacklogLevel() { }

    public BacklogLevel(string name, string? description, byte order)
    {
        Name = name.Trim();
        Description = description?.Trim();
        Order = order;
    }

    /// <summary>
    /// The name of the backlog level.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// The description of the backlog level.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// The order in which the backlog levels are displayed. The lower the number, the higher the level. 
    /// The minimum value is 0 and the maximum value is 255.
    /// </summary>
    public byte Order { get; private set; }

    /// <summary>
    /// Indicates whether the backlog level is active or not.  
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// The process for updating the backlog level properties.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public Result Update(string name, string? description, byte order)
    {
        Name = name.Trim();
        Description = description?.Trim();
        Order = order;

        return Result.Success();
    }

    /// <summary>
    /// The process for activating a backlog level.
    /// </summary>
    /// <param name="activatedOn"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Activate(Instant activatedOn)
    {
        if (!IsActive)
        {
            // TODO is there logic that would prevent activation?
            IsActive = true;
            AddDomainEvent(EntityActivatedEvent.WithEntity(this, activatedOn));
        }

        return Result.Success();
    }

    /// <summary>
    /// The process for deactivating a backlog level.
    /// </summary>
    /// <param name="deactivatedOn"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Deactivate(Instant deactivatedOn)
    {
        if (IsActive)
        {
            // TODO is there logic that would prevent deactivation?
            IsActive = false;
            AddDomainEvent(EntityDeactivatedEvent.WithEntity(this, deactivatedOn));
        }

        return Result.Success();
    }
}
