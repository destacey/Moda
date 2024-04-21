using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;

namespace Moda.Common.Models;

public class EmailAddress : ValueObject
{
    public EmailAddress(string value)
    {
        value = Guard.Against.NullOrWhiteSpace(value, nameof(EmailAddress)).Trim();

        if (ValidateEmailAddressFormat(value))
        {
            Value = value;
        }
    }

    public string Value { get; } = null!;

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Value;
    }

    // only validates that the format is correct
    private bool ValidateEmailAddressFormat(string value)
    {
        return value.IsValidEmailAddressFormat()
            ? true
            : throw new ArgumentException("The value submitted does not meet the required format.", nameof(EmailAddress));
    }

    public override string ToString() => Value;

    public static implicit operator string(EmailAddress emailAddress) => emailAddress.Value;
    public static explicit operator EmailAddress(string value) => new(value);
}