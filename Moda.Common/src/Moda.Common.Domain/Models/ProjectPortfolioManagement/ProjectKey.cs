using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;

namespace Moda.Common.Domain.Models.ProjectPortfolioManagement;

/// <summary>
/// Represents a unique project key used for task key generation (e.g., "APOLLO", "MARS1").
/// Must be 2-20 uppercase alphanumeric characters.
/// </summary>
public sealed class ProjectKey : ValueObject
{
    public const string ValidationRegex = "^[A-Z0-9]{2,20}$";

    public ProjectKey(string value)
    {
        value = Guard.Against.NullOrWhiteSpace(value, nameof(ProjectKey)).Trim().ToUpper();

        ValidateFormat(value);

        Value = value;
    }

    public string Value { get; }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Value;
    }

    private bool ValidateFormat(string value)
    {
        return Regex.IsMatch(value, ValidationRegex)
            ? true
            : throw new ArgumentException($"The value '{value}' does not meet the required format for a project key. Must be 2-20 uppercase alphanumeric characters.", nameof(ProjectKey));
    }

    public static implicit operator string(ProjectKey projectKey) => projectKey.Value;
    public static explicit operator ProjectKey(string value) => new(value);
}
