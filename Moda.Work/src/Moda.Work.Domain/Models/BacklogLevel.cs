using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using NodaTime;

namespace Moda.Work.Domain.Models;

/// <summary>Allows work types to be grouped and defined in a hierarchy.</summary>
/// <seealso cref="Moda.Common.Domain.Data.BaseAuditableEntity&lt;System.Int32&gt;" />
/// <seealso cref="Moda.Common.Domain.Interfaces.IActivatable&lt;NodaTime.Instant&gt;" />
public sealed class BacklogLevel : BaseAuditableEntity<int>
{
    private string _name = null!;
    private string? _description;

    private BacklogLevel() { }
    
    private BacklogLevel(string name, string? description, BacklogCategory category, Ownership ownership, int rank)
    {
        Name = name;
        Description = description;
        Category = category;
        Ownership = ownership;
        Rank = rank;
    }

    public Guid ParentId { get; init; }

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

    public BacklogCategory Category { get; init; }

    /// <summary>
    /// Indicates whether the backlog level is owned by Moda or a third party system.  This value should not change.
    /// </summary>
    /// <value>The ownership.</value>
    public Ownership Ownership { get; init; }

    /// <summary>
    /// The rank of the backlog level.  The higher the rank, the higher the priority.
    /// </summary>
    /// <value>The rank.</value>
    public int Rank { get; private set; }

    /// <summary>Updates the specified name.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="rank">The rank.</param>
    /// <param name="timestamp">The timestamp.</param>
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

    /// <summary>Creates the specified BacklogLevel.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="category">The category.</param>
    /// <param name="ownership">The ownership.</param>
    /// <param name="rank">The rank.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns></returns>
    public static BacklogLevel Create(string name, string? description, BacklogCategory category, Ownership ownership, int rank, Instant timestamp)
    {
        BacklogLevel backlogLevel = new(name, description, category, ownership, rank);

        backlogLevel.AddDomainEvent(EntityCreatedEvent.WithEntity(backlogLevel, timestamp));
        return backlogLevel;
    }
}