using FluentValidation;
using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Work.Application.WorkTypes.Validators;
public sealed class IExternalWorkTypeValidator : CustomValidator<IExternalWorkType>
{
    public IExternalWorkTypeValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}
