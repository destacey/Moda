using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;

namespace Moda.Common.Domain.Models;

/// <summary>
/// Lightweight instance wrapper around a metadata collection that delegates behavior to <see cref="MetadataManager{TMetadata}"/>.
/// Use this when you want to expose an instance API (e.g. <c>iteration.ExternalMetadataManager.Upsert(...)</c>)
/// without copying the Upsert/Remove logic into each aggregate.
/// </summary>
public sealed class MetadataAccessor<TMetadata> where TMetadata : KeyValueObjectMetadata
{
    private readonly Func<Guid> _objectIdProvider;
    private readonly Func<ICollection<TMetadata>> _collectionProvider;
    private readonly Func<Guid, string, string?, TMetadata>? _factory;

    public MetadataAccessor(Func<Guid> objectIdProvider, Func<ICollection<TMetadata>> collectionProvider, Func<Guid, string, string?, TMetadata>? factory = null)
    {
        _objectIdProvider = Guard.Against.Null(objectIdProvider, nameof(objectIdProvider));
        _collectionProvider = Guard.Against.Null(collectionProvider, nameof(collectionProvider));
        _factory = factory;
    }

    private ICollection<TMetadata> Collection => _collectionProvider();

    /// <summary>
    /// Upserts a single metadata entry into the underlying collection.
    /// </summary>
    public Result Upsert(string name, string? value)
        => MetadataManager<TMetadata>.Upsert(Collection, _objectIdProvider(), name, value, _factory);

    /// <summary>
    /// Upserts multiple metadata entries into the underlying collection.
    /// </summary>
    public Result UpsertMany(IEnumerable<(string Name, string? Value)> items)
        => MetadataManager<TMetadata>.UpsertMany(Collection, _objectIdProvider(), items, _factory);

    /// <summary>
    /// Removes a metadata entry by name.
    /// </summary>
    public Result Remove(string name)
        => MetadataManager<TMetadata>.Remove(Collection, name);

    /// <summary>
    /// Gets an entry by name or null if not present.
    /// </summary>
    public TMetadata? Get(string name)
        => MetadataManager<TMetadata>.Get((IReadOnlyCollection<TMetadata>)Collection, name);

    /// <summary>
    /// Returns a snapshot of the underlying metadata collection.
    /// </summary>
    public IReadOnlyCollection<TMetadata> GetAll() => (IReadOnlyCollection<TMetadata>)Collection;
}