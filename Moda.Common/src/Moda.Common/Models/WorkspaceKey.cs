using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;

namespace Moda.Common.Models;
public sealed class WorkspaceKey : ValueObject
{
    /// <summary>
    /// The regular expression used to validate the workspace key format.
    /// The workspace key must be uppercase letters and numbers only, start with an uppercase letter, and have a length between 2-20 characters.
    /// </summary>
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
    private bool ValidateWorkspaceKeyFormat(string value)
    {
        return value.IsValidWorkspaceKeyFormat()
            ? true
            : throw new ArgumentException("The value submitted does not meet the required format.", nameof(WorkspaceKey));
    }

    public static implicit operator string(WorkspaceKey workspaceKey) => workspaceKey.Value;
    public static explicit operator WorkspaceKey(string value) => new(value);
}
