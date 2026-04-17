using Wayd.Common.Application.Interfaces.ExternalWork;
using Wayd.Common.Application.Validators;

namespace Wayd.Common.Application.Requests.WorkManagement.Commands;

public sealed record CreateExternalWorkflowCommand(string Name, string? Description, IExternalWorkTypeWorkflow ExternalWorkTypeWorkflow) : ICommand<Guid>;

public sealed class CreateExternalWorkflowCommandValidator : CustomValidator<CreateExternalWorkflowCommand>
{
    public CreateExternalWorkflowCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(c => c.Description)
            .MaximumLength(1024);

        RuleFor(c => c.ExternalWorkTypeWorkflow)
            .NotNull()
            .SetValidator(new IExternalWorkTypeWorkflowValidator());
    }
}

