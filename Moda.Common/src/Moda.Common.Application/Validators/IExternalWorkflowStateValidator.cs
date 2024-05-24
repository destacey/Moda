using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Common.Application.Validators;
public sealed class IExternalWorkflowStateValidator : CustomValidator<IExternalWorkflowState>
{
    public IExternalWorkflowStateValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Continue;

        RuleFor(s => s.StatusName)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(s => s.Category)
            .IsInEnum()
            .WithMessage("A valid category must be selected.");

        RuleFor(s => s.Order)
            .NotNull();

        RuleFor(s => s.IsActive)
            .NotNull();
    }
}
