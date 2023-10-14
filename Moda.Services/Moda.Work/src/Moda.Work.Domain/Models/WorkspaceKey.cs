using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Work.Domain.Extensions;

namespace Moda.Work.Domain.Models;
public sealed class WorkspaceKey : ValueObject
{
    internal const string Regex = "^([A-Z][A-Z0-9]{1,19})$";

    public WorkspaceKey(string value)
    {
        value = Guard.Against.NullOrWhiteSpace(value, nameof(WorkspaceKey)).Trim().ToUpper();

        if (ValidateWorkspaceKeyFormat(value))
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
    private bool ValidateWorkspaceKeyFormat(string value) => value.IsValidWorkspaceKeyFormat()
            ? true
            : throw new ArgumentException("The value submitted does not meet the required format.", nameof(WorkspaceKey));

    public static implicit operator string(WorkspaceKey workspaceKey) => workspaceKey.Value;
    public static explicit operator WorkspaceKey(string value) => new(value);
}
