using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Common.Application.Validators;
public sealed class IExternalWorkStatusValidator : CustomValidator<IExternalWorkStatus>
{
    public IExternalWorkStatusValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(64);
    }
}
