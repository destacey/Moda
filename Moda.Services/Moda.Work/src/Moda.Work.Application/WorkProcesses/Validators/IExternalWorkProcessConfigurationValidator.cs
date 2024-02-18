using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Work.Application.WorkProcesses.Validators;
public sealed class IExternalWorkProcessConfigurationValidator : CustomValidator<IExternalWorkProcessConfiguration>
{
    public IExternalWorkProcessConfigurationValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty();

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}
