using Wayd.Common.Application.Interfaces.ExternalWork;

namespace Wayd.Common.Application.Validators;

public sealed class IExternalWorkStatusValidator : CustomValidator<IExternalWorkStatus>
{
    public IExternalWorkStatusValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(64);
    }
}
