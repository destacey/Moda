using Ardalis.GuardClauses;

namespace Moda.Common.Domain.Models;
public class KeyValueObjectMetadata : ISystemAuditable
{
    private KeyValueObjectMetadata() { }

    public KeyValueObjectMetadata(Guid objectId, string name, string? value)
    {
        ObjectId = objectId;
        Name = Guard.Against.NullOrWhiteSpace(name, nameof(name)).Trim();
        Value = string.IsNullOrWhiteSpace(value) ? null : value?.Trim();
    }

    public Guid ObjectId { get; private init; }
    public string Name { get; private set; } = null!;
    public string? Value { get; private set; }

    public void UpdateValue(string? value) => Value = value;
}
