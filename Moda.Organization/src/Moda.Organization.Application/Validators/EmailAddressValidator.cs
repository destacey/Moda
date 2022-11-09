using FluentValidation;

namespace Moda.Organization.Application.Validators;
public sealed class EmailAddressValidator : CustomValidator<EmailAddress>
{
    public EmailAddressValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(e => e.Value)
            .NotEmpty()
            .MaximumLength(256);
    }
}
