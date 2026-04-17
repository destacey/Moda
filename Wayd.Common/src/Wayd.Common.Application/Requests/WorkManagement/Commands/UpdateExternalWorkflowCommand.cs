using Wayd.Common.Application.Interfaces.ExternalWork;
using Wayd.Common.Application.Validators;

namespace Wayd.Common.Application.Requests.WorkManagement.Commands;

public sealed record UpdateExternalWorkflowCommand(Guid WorkflowId, string Name, string? Description, IExternalWorkTypeWorkflow ExternalWorkTypeWorkflow) : ICommand, ILongRunningRequest;

public sealed class UpdateExternalWorkflowCommandValidator : CustomValidator<UpdateExternalWorkflowCommand>
{
    public UpdateExternalWorkflowCommandValidator()
    {
        RuleFor(c => c.WorkflowId)
            .NotEmpty();

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
