using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;

namespace Moda.ProjectPortfolioManagement.Domain.Models;

/// <summary>
/// Represents a unique project code used for task key generation (e.g., "APOLLO", "MARS-1").
/// Must be 2-20 uppercase alphanumeric characters or hyphens.
/// </summary>
public sealed class ProjectCode : ValueObject
{
    public const string ValidationRegex = "^[A-Z0-9-]{2,20}$";

    public ProjectCode(string value)
    {
        value = Guard.Against.NullOrWhiteSpace(value, nameof(ProjectCode)).Trim().ToUpper();

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
            : throw new ArgumentException($"The value '{value}' does not meet the required format for a project code. Must be 2-20 uppercase alphanumeric characters or hyphens.", nameof(ProjectCode));
    }

    public static implicit operator string(ProjectCode projectCode) => projectCode.Value;
    public static explicit operator ProjectCode(string value) => new(value);
}
