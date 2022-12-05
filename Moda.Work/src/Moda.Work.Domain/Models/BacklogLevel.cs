using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using NodaTime;

namespace Moda.Work.Domain.Models;

/// <summary>Allows work types to be grouped and defined in a hierarchy.</summary>
/// <seealso cref="Moda.Common.Domain.Data.BaseAuditableEntity&lt;System.Int32&gt;" />
/// <seealso cref="Moda.Common.Domain.Interfaces.IActivatable" />
public sealed class BacklogLevel : BaseAuditableEntity<int>, IActivatable
{
    private string _name = null!;
    private string? _description;

    private BacklogLevel() { }
    
    private BacklogLevel(string name, string? description, int rank)
    {
        Name = name;
        Description = description;
        Rank = rank;
    }

    /// <summary>The name of the backlog level.</summary>
    /// <value>The name.</value>
    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>The description of the backlog level.</summary>
    /// <value>The description.</value>
    public string? Description
    {
        get => _description;
        private set => _description = value?.Trim();
    }

    /// <summary>
    /// The rank of the backlog level. The higher the number, the higher the level.
    /// </summary>
    /// <value>The rank.</value>
    public int Rank { get; private set; }

    /// <summary>Indicates whether the backlog level is active or not.</summary>
    /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; private set; } = true;

    /// <summary>The process for updating the backlog level properties.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="rank">The rank.</param>
    /// <returns></returns>
    public Result Update(string name, string? description, int rank, Instant timestamp)
    {
        try
        {
            Name = name;
            Description = description;
            Rank = rank;

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    /// <summary>
    /// The process for activating a backlog level.
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
    /// The process for deactivating a backlog level.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Deactivate(Instant timestamp)
    {
        if (IsActive)
        {
            // TODO is there logic that would prevent deactivation?
            IsActive = false;
            AddDomainEvent(EntityDeactivatedEvent.WithEntity(this, timestamp));
        }

        return Result.Success();
    }

    /// <summary>Creates the specified BacklogLevel.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="rank">The rank.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns></returns>
    public static BacklogLevel Create(string name, string? description, int rank, Instant timestamp)
    {
        BacklogLevel backlogLevel = new(name, description, rank);

        backlogLevel.AddDomainEvent(EntityCreatedEvent.WithEntity(backlogLevel, timestamp));
        return backlogLevel;
    }
}
