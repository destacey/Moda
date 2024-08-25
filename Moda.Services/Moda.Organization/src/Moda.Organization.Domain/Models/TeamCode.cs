using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Organization.Domain.Extensions;

namespace Moda.Organization.Domain.Models;

// TODO: move to Moda.Common.Domain
public class TeamCode : ValueObject
{
    public static string Regex = "^([A-Z0-9]){2,10}$";

    public TeamCode(string value)
    {
        value = Guard.Against.NullOrWhiteSpace(value, nameof(TeamCode)).Trim().ToUpper();

        ValidateOrganizationCodeFormat(value);

        Value = value;
    }

    public string Value { get; }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Value;
    }

    // only validates that the format is correct
    private bool ValidateOrganizationCodeFormat(string value)
    {
        return value.IsValidTeamCodeFormat()
            ? true
            : throw new ArgumentException("The value submitted does not meet the required format.", nameof(TeamCode));
    }

    public static implicit operator string(TeamCode organizationCode) => organizationCode.Value;
    public static explicit operator TeamCode(string value) => new(value);
}
