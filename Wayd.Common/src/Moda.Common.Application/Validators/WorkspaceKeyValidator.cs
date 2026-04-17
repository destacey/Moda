using Wayd.Common.Models;

namespace Wayd.Common.Application.Validators;

public sealed class WorkspaceKeyValidator : CustomValidator<WorkspaceKey>
{
    public WorkspaceKeyValidator()
    {
        RuleFor(w => w.Value)
            .NotEmpty()
            .MaximumLength(20);
    }
}
