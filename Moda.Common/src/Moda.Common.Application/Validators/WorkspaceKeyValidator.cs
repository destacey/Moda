using Moda.Common.Models;

namespace Moda.Common.Application.Validators;
public sealed class WorkspaceKeyValidator : CustomValidator<WorkspaceKey>
{
    public WorkspaceKeyValidator()
    {
        RuleFor(w => w.Value)
            .NotEmpty()
            .MaximumLength(20);
    }
}
