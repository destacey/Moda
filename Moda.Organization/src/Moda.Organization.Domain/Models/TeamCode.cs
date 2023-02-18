using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Organization.Domain.Extensions;

namespace Moda.Organization.Domain.Models;
public class TeamCode : ValueObject
{
    public TeamCode(string value)
    {
        value = Guard.Against.NullOrWhiteSpace(value, nameof(TeamCode)).Trim().ToUpper();

        if (ValidateOrganizationCodeFormat(value))
        {
            Value = value;
        }
    }

    public string Value { get; } = null!;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    // only validates that the format is correct
    private bool ValidateOrganizationCodeFormat(string value) => value.IsValidTeamCodeFormat()
            ? true
            : throw new ArgumentException("The value submitted does not meet the required format.", nameof(TeamCode));

    public static implicit operator string(TeamCode organizationCode) => organizationCode.Value;
    public static explicit operator TeamCode(string value) => new(value);
}
