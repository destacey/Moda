using Wayd.Common.Application.Interfaces.ExternalWork;

namespace Wayd.Common.Application.Validators;

public sealed class IExternalWorkTypeWorkflowValidator : CustomValidator<IExternalWorkTypeWorkflow>
{
    public IExternalWorkTypeWorkflowValidator()
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

        RuleForEach(e => e.WorkflowStates)
            .NotNull()
            .SetValidator(new IExternalWorkflowStateValidator());
    }
}
