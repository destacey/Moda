using Ardalis.GuardClauses;

namespace Wayd.Work.Domain.Models;

public record WorkItemTag
{
    public WorkItemTag(string value)
    {
        Value = Guard.Against.NullOrWhiteSpace(value, nameof(value)).Trim();
    }

    public string Value { get; }
}
