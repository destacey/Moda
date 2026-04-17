using Wayd.Common.Application.Interfaces.ExternalWork;

namespace Wayd.Common.Application.Validators;

public sealed class IExternalWorkspaceConfigurationValidator : CustomValidator<IExternalWorkspaceConfiguration>
{
    public IExternalWorkspaceConfigurationValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty();

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(c => c.Description)
            .MaximumLength(1024);

        RuleFor(c => c.WorkProcessId)
            .NotEmpty();
    }
}
