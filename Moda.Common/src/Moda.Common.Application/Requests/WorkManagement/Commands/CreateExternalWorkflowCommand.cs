using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Validators;

namespace Moda.Common.Application.Requests.WorkManagement.Commands;
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

