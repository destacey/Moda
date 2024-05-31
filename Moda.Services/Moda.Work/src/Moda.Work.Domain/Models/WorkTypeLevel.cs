using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Enums.Work;
using Moda.Common.Extensions;
using NodaTime;

namespace Moda.Work.Domain.Models;

/// <summary>Allows work types to be grouped and defined in a hierarchy.</summary>
/// <seealso cref="Moda.Common.Domain.Data.BaseAuditableEntity&lt;System.Int32&gt;" />
public sealed class WorkTypeLevel : BaseEntity<int>, ISystemAuditable
{
    private string _name = null!;
    private string? _description;

    private WorkTypeLevel() { }

    private WorkTypeLevel(string name, string? description, WorkTypeTier tier, Ownership ownership, int order)
    {
        Name = name;
        Description = description;
        Tier = tier;
        Ownership = ownership;
        Order = order;
    }

    /// <summary>The name of the work type level.</summary>
    /// <value>The name.</value>
    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>The description of the work type level.</summary>
    /// <value>The description.</value>
    public string? Description
    {
        get => _description;
        private set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    public WorkTypeTier Tier { get; private init; }

    /// <summary>
    /// Indicates whether the work type level is owned by Moda or a third party system.  This value can not change.
    /// </summary>
    /// <value>The ownership.</value>
    public Ownership Ownership { get; private init; }

    /// <summary>
    /// The order of the work type level.
    /// </summary>
    /// <value>The order.</value>
    public int Order { get; private set; }

    /// <summary>Updates the specified name.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="order">The rank.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns></returns>
    public Result Update(string name, string? description, int order, Instant timestamp)
    {
        try
        {
            Name = name;
            Description = description;
            Order = order;

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    /// <summary>
    /// Creates a new work type level.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="tier"></param>
    /// <param name="ownership"></param>
    /// <param name="order"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static WorkTypeLevel Create(string name, string? description, WorkTypeTier tier, Ownership ownership, int order, Instant timestamp)
    {
        WorkTypeLevel workTypeLevel = new(name, description, tier, ownership, order);

        workTypeLevel.AddDomainEvent(EntityCreatedEvent.WithEntity(workTypeLevel, timestamp));
        return workTypeLevel;
    }
}