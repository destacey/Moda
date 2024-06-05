using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Common.Application.Validators;
public sealed class IExternalWorkTypeValidator : CustomValidator<IExternalWorkType>
{
    public IExternalWorkTypeValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Continue;

        RuleFor(e => e.Name)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(e => e.Description)
            .MaximumLength(1024);

        RuleFor(e => e.WorkTypeLevelId)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(e => e.IsActive)
            .NotNull();
    }
}
