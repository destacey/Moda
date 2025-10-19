using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Enums.Planning;
using Moda.Common.Domain.Interfaces;
using Moda.Common.Domain.Models;

namespace Moda.Planning.Domain.Models.Iterations;
public class Iteration : BaseEntity<Guid>, ISystemAuditable, IHasIdAndKey
{
    private string _name = default!;
    private readonly List<KeyValueObjectMetadata> _externalMetadata = [];

    private Iteration() { }

    private Iteration(string name, IterationType type, IterationState state, IterationDateRange dateRange, Guid? teamId, OwnershipInfo ownershipInfo, List<KeyValueObjectMetadata> externalMetadata)
    {
        Name = name;
        Type = type;
        State = state;
        DateRange = dateRange;
        TeamId = teamId;
        OwnershipInfo = ownershipInfo ?? throw new ArgumentNullException(nameof(ownershipInfo));
        _externalMetadata = externalMetadata ?? [];
    }

    /// <summary>
    /// The unique key of the Iteration.  This is an alternate key to the Id.
    /// </summary>
    public int Key { get; private init; }

    /// <summary>
    /// The name of the Iteration.
    /// </summary>
    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>
    /// The type of iteration being performed.
    /// </summary>
    public IterationType Type { get; private set; }

    /// <summary>
    /// The current state of the iteration.
    /// </summary>
    public IterationState State { get; private set; }

    /// <summary>
    /// The date range of the iteration.
    /// </summary>
    public IterationDateRange DateRange { get; private set; } = default!;

    /// <summary>
    /// The team that owns this iteration, if applicable.
    /// </summary>
    public Guid? TeamId { get; private set; }

    /// <summary>
    /// The team that owns this iteration, if applicable.
    /// </summary>
    public PlanningTeam? Team { get; private set; }

    /// <summary>
    /// The ownership information for this iteration.
    /// </summary>
    public OwnershipInfo OwnershipInfo { get; set; } = default!;
    
    /// <summary>
    /// Gets a read-only collection of external metadata associated with the object.
    /// </summary>
    public IReadOnlyCollection<KeyValueObjectMetadata> ExternalMetadata => _externalMetadata.AsReadOnly();

    public Result Update(string name, IterationType type, IterationState state, IterationDateRange dateRange, Guid? teamId)
    {
        Name = name;
        Type = type;
        State = state;
        DateRange = dateRange;
        TeamId = teamId;

        return Result.Success();
    }

    /// <summary>
    /// Manager instance that exposes Upsert/Remove/Get APIs for external metadata.
    /// Example usage: <c>iteration.ExternalMetadataManager.Upsert("azdo.path", "/Team/Iteration")</c>
    /// </summary>
    public MetadataAccessor<KeyValueObjectMetadata> ExternalMetadataManager
        => new(() => Id, () => _externalMetadata);

    /// <summary>
    /// Creates a new Iteration instance.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="state"></param>
    /// <param name="dateRange"></param>
    /// <param name="teamId"></param>
    /// <param name="ownershipInfo"></param>
    /// <param name="externalMetadata"></param>
    /// <returns></returns>
    public static Iteration Create(string name, IterationType type, IterationState state, IterationDateRange dateRange, Guid? teamId, OwnershipInfo ownershipInfo, List<KeyValueObjectMetadata> externalMetadata)
        => new(name, type, state, dateRange, teamId, ownershipInfo, externalMetadata);
}
