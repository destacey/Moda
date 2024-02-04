using System.Diagnostics.CodeAnalysis;

namespace Moda.Common.Domain.Models;
public sealed class IntegrationState<TId>
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private IntegrationState() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [SetsRequiredMembers]
    private IntegrationState(TId internalId, bool isActive)
    {
        InternalId = internalId;
        IsActive = isActive;
    }

    public TId InternalId { get; private init; }
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
