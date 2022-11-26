using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using NodaTime;

namespace Moda.Work.Domain.Models;

public sealed class WorkState : BaseAuditableEntity<int>, IAggregateRoot, IActivatable
{
    private string _name = null!;
    private string? _description;

    private WorkState() { }

    private WorkState(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    /// <summary>The name of the work state.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public string Name
    {
        get => _name;
        init => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }
    
    /// <summary>The description of the work state.</summary>
    /// <value>The description.</value>
    public string? Description
    {
        get => _description;
        private set => _description = value?.Trim();
    }

    /// <summary>Indicates whether the work state is active or not.</summary>
    /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; private set; } = true;

    /// <summary>Updates the specified work state.</summary>
    /// <param name="description">The description.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns></returns>
    public Result Update(string? description, Instant timestamp)
    {
        try
        {
            Description = description;

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    /// <summary>
    /// The process for activating a work state.
    /// </summary>
    /// <param name="activatedOn"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Activate(Instant activatedOn)
    {
        if (!IsActive)
        {
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
            IsActive = false;
            AddDomainEvent(EntityDeactivatedEvent.WithEntity(this, deactivatedOn));
        }

        return Result.Success();
    }

    /// <summary>Creates the work state.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns></returns>
    public static WorkState Create(string name, string? description, Instant timestamp)
    {
        WorkState workState = new(name, description);

        workState.AddDomainEvent(EntityCreatedEvent.WithEntity(workState, timestamp));
        return workState;
    }
}
