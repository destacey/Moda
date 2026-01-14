using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Moda.Common.Domain.Models;
public sealed class IntegrationState<TId>
{
    private IntegrationState() { }

    [JsonConstructor]
    [SetsRequiredMembers]
    private IntegrationState(TId internalId, bool isActive)
    {
        InternalId = internalId;
        IsActive = isActive;
    }

    public TId InternalId { get; private init; } = default!;
    public bool IsActive { get; private set; }

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }

    public static IntegrationState<TId> Create(TId internalId, bool isActive)
    {
        return new IntegrationState<TId>(internalId, isActive);
    }
}
