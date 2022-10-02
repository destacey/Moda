using CSharpFunctionalExtensions;
using NodaTime;

namespace Moda.Work.Domain.Models;

public sealed class WorkState : BaseAuditableEntity<Guid>, IAggregateRoot, IActivatable
{
    private WorkState() { }

    public WorkState(string name, string? description)
    {
        Name = name.Trim();
        Description = description?.Trim();
    }

    /// <summary>
    /// The name of the work state.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// The description of the work state.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Indicates whether the work state is active or not.  
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// The process for activating a work state.
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
    /// The process for deactivating a work state.
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
