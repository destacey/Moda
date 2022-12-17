using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using NodaTime;

namespace Moda.Work.Domain.Models;

/// <summary>
/// Represents a type of work item.
/// </summary>
/// <seealso cref="Moda.Common.Domain.Data.BaseAuditableEntity&lt;System.Int32&gt;" />
/// <seealso cref="Moda.Common.Domain.Interfaces.IActivatable&lt;NodaTime.Instant&gt;" />
public sealed class WorkType : BaseAuditableEntity<int>, IActivatable<Instant>
{
    private string _name = null!;
    private string? _description;

    private WorkType() { }

    private WorkType(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    /// <summary>The name of the work type.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public string Name
    {
        get => _name;
        init => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>The description of the work type.</summary>
    /// <value>The description.</value>
    public string? Description
    {
        get => _description;
        private set => _description = value?.Trim();
    }

    /// <summary>Indicates whether the work type is active or not.</summary>
    /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; private set; } = true;

    /// <summary>Updates the specified work type.</summary>
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
    /// The process for activating a work type.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Activate(Instant timestamp)
    {
        if (!IsActive)
        {
            // TODO is there logic that would prevent activation?
            IsActive = true;
            AddDomainEvent(EntityActivatedEvent.WithEntity(this, timestamp));
        }

        return Result.Success();
    }

    /// <summary>
    /// The process for deactivating a work type.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Deactivate(Instant timestamp)
    {
        if (IsActive)
        {
            IsActive = false;
            AddDomainEvent(EntityDeactivatedEvent.WithEntity(this, timestamp));
        }

        return Result.Success();
    }

    /// <summary>Creates the a work type.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns></returns>
    public static WorkType Create(string name, string? description, Instant timestamp)
    {
        WorkType workType = new(name, description);

        workType.AddDomainEvent(EntityCreatedEvent.WithEntity(workType, timestamp));
        return workType;
    }
}
