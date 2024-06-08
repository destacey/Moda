using Moda.Common.Application.Interfaces.Work;
using Moda.Common.Application.Validators;

namespace Moda.Common.Application.Requests.WorkManagement;
public sealed record SyncExternalWorkItemsCommand(Guid WorkspaceId, List<IExternalWorkItem> WorkItems) : ICommand;

public sealed class SyncExternalWorkItemsCommandValidator : CustomValidator<SyncExternalWorkItemsCommand>
{
    public SyncExternalWorkItemsCommandValidator()
    {
        RuleFor(c => c.WorkspaceId)
            .NotEmpty();

        RuleForEach(c => c.WorkItems)
            .NotNull()
            .SetValidator(new IExternalWorkItemValidator());
    }
}
