using CSharpFunctionalExtensions;

namespace Moda.Common.Models;

public class EmailAddress : ValueObject
{
    private readonly string _nameOf = nameof(EmailAddress);

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public EmailAddress(string value)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        if (ValidateEmailAddressFormat(value))
        {
            Value = value;
        }
    }

    public string Value { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    // only validates that the format is correct
    private bool ValidateEmailAddressFormat(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(_nameOf, $"{_nameOf} can not be null.");
        }

        if (value.IsValidEmailAddressFormat())
        {
            return true;
        }

        throw new ArgumentException("The value submitted does not meet the required format.", _nameOf);
    }

    public override string ToString()
    {
        return Value;
    }
}
