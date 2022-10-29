using CSharpFunctionalExtensions;

namespace Moda.Common.Models;

public class EmailAddress : ValueObject
{
    private readonly string _nameOf = nameof(EmailAddress);

    public EmailAddress(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, _nameOf);

        value = value.Trim();

        if (ValidateEmailAddressFormat(value))
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
    private bool ValidateEmailAddressFormat(string value)
    {
        if (value.IsValidEmailAddressFormat())
            return true;

        throw new ArgumentException("The value submitted does not meet the required format.", _nameOf);
    }

    public override string ToString()
    {
        return Value;
    }
}
