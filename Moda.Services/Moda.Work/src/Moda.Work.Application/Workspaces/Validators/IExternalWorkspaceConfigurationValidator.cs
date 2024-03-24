using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Work.Application.Workspaces.Validators;
public sealed class IExternalWorkspaceConfigurationValidator : CustomValidator<IExternalWorkspaceConfiguration>
{
    public IExternalWorkspaceConfigurationValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty();

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(c => c.Description)
            .MaximumLength(1024);

        RuleFor(c => c.WorkProcessId)
            .NotEmpty();
    }
}
