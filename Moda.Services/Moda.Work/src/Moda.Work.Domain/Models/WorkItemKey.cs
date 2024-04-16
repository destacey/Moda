using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Models;
using Moda.Work.Domain.Extensions;

namespace Moda.Work.Domain.Models;
public sealed class WorkItemKey : ValueObject
{
    /// <summary>
    /// The regular expression used to validate the work item key format.
    /// The Work Item key consists of two groups separated by a hyphen.  The first group is a WorkspaceKey and the second group is a number. Example: "WS-1234"
    /// </summary>
    internal const string Regex = "^([A-Z][A-Z0-9]{1,19})-(\\d+)$";

    /// <summary>
    /// Used when deserializing from a string.
    /// </summary>
    /// <param name="value"></param>
    public WorkItemKey(string value)
    {
        Guard.Against.NullOrWhiteSpace(value, nameof(WorkItemKey));

        if (ValidateWorkItemKeyFormat(value))
        {
            Value = value;
        }
    }

    public WorkItemKey(WorkspaceKey workspaceKey, int number)
    {
        Guard.Against.NullOrWhiteSpace(workspaceKey);

        string value = $"{workspaceKey}-{number}";

        if (ValidateWorkItemKeyFormat(value))
        {
            Value = value;
        }
    }

    public string Value { get; init; } = null!;

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Value;
    }

    // only validates that the format is correct
    private bool ValidateWorkItemKeyFormat(string value)
    {
        return value.IsValidWorkItemKeyFormat()
            ? true
            : throw new ArgumentException("The value submitted does not meet the required format.", nameof(WorkItemKey));
    }

    public override string ToString() => Value;

    public static implicit operator string(WorkItemKey workItemKey) => workItemKey.Value;
    public static explicit operator WorkItemKey(string value) => new(value);
}
