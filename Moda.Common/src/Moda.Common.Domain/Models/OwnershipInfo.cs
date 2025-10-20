using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Enums;

namespace Moda.Common.Domain.Models;
public sealed class OwnershipInfo : ValueObject
{
    private string? _systemId;
    private string? _externalId;

    private OwnershipInfo() { }
    private OwnershipInfo(Ownership ownership, string? systemId = null, string? externalId = null)
    {
        Ownership = ownership;
        SystemId = systemId;
        ExternalId = externalId;

        if (ownership is Ownership.Managed)
        {
            if (string.IsNullOrWhiteSpace(systemId))
                throw new ArgumentException("The system identifier is required when the ownership is managed.", nameof(systemId));
            if (string.IsNullOrWhiteSpace(externalId))
                throw new ArgumentException("The external identifier is required when the ownership is managed.", nameof(externalId));
        }
    }

    /// <summary>
    /// The ownership type of the entity.
    /// </summary>
    public Ownership Ownership { get; private init; }

    /// <summary>
    /// The unique identifier of the external system that manages this entity. This is required when Ownership is Managed.
    /// </summary>
    public string? SystemId 
    { 
        get => _systemId;
        private init => _systemId = Guard.Against.NullOrWhiteSpace(value, nameof(SystemId)).Trim();
    }

    /// <summary>
    /// The unique identifier of the entity in the external system that manages this entity. This is required when Ownership is Managed.
    /// </summary>
    public string? ExternalId
    {
        get => _externalId;
        private init => _externalId = Guard.Against.NullOrWhiteSpace(value, nameof(ExternalId)).Trim();
    }

    /// <summary>
    /// Creates a Moda owned instance of OwnershipInfo. 
    /// </summary>
    /// <returns></returns>
    public static OwnershipInfo CreateModaOwned()
        => new(Ownership.Owned);

    /// <summary>
    /// Creates a managed instance of OwnershipInfo. This is used for entities that are managed by an external system.
    /// </summary>
    /// <param name="systemId"></param>
    /// <param name="externalId"></param>
    /// <returns></returns>
    public static OwnershipInfo CreateExternalOwned(string systemId, string externalId) 
        => new (Ownership.Managed, systemId, externalId);

    /// <summary>
    /// Creates a system owned instance of OwnershipInfo. This is used for system defined entities that should not be modified or deleted by users.
    /// </summary>
    /// <returns></returns>
    public static OwnershipInfo CreateSystemOwned()
        => new(Ownership.System);

    /// <summary>
    /// Gets the equality components.
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Ownership;

        if (SystemId is not null)
            yield return SystemId;

        if (ExternalId is not null)
            yield return ExternalId;
    }

}
