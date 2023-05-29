using Moda.Common.Models;

namespace Moda.Common.Application.Validators;
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
