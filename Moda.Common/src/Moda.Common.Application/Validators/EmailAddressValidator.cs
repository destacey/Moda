using Wayd.Common.Models;

namespace Wayd.Common.Application.Validators;

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
