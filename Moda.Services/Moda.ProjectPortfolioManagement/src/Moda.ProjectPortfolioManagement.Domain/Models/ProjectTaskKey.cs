using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;

namespace Moda.ProjectPortfolioManagement.Domain.Models;

/// <summary>
/// Represents a unique identifier for a project task in the format {ProjectCode}-T{Number}.
/// </summary>
public sealed class ProjectTaskKey : ValueObject
{
    internal const string ValidationRegex = "^([A-Z0-9-]{2,20})-T(\\d+)$";

    private ProjectTaskKey() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectTaskKey"/> class from a string value.
    /// </summary>
    /// <param name="value">The task key value (e.g., "APOLLO-T001").</param>
    public ProjectTaskKey(string value)
    {
        Guard.Against.NullOrWhiteSpace(value, nameof(value));

        if (!ValidateFormat(value))
        {
            throw new ArgumentException($"The value '{value}' does not meet the required format for a project task key. Expected format: PROJECT-T###", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectTaskKey"/> class from a project code and task number.
    /// </summary>
    /// <param name="projectCode">The project code.</param>
    /// <param name="taskNumber">The task number.</param>
    public ProjectTaskKey(ProjectCode projectCode, int taskNumber)
    {
        Guard.Against.Null(projectCode, nameof(projectCode));
        Guard.Against.NegativeOrZero(taskNumber, nameof(taskNumber));

        string value = $"{projectCode.Value}-T{taskNumber}";

        if (!ValidateFormat(value))
        {
            throw new ArgumentException($"The project code '{projectCode.Value}' does not meet the required format. Must be 2-20 uppercase alphanumeric characters or hyphens.", nameof(projectCode));
        }

        Value = value;
    }

    /// <summary>
    /// Gets the full task key value (e.g., "APOLLO-T001").
    /// </summary>
    public string Value { get; init; } = default!;

    /// <summary>
    /// Gets the project code portion of the task key (e.g., "APOLLO").
    /// </summary>
    public string ProjectCode => Value.Split("-T")[0];

    /// <summary>
    /// Gets the task number portion of the task key (e.g., 1 from "APOLLO-T001").
    /// </summary>
    public int TaskNumber => int.Parse(Value.Split("-T")[1]);

    /// <summary>
    /// Validates the format of a task key value.
    /// </summary>
    private static bool ValidateFormat(string value)
    {
        return Regex.IsMatch(value, ValidationRegex);
    }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(ProjectTaskKey key) => key.Value;
    public static explicit operator ProjectTaskKey(string value) => new(value);
}
