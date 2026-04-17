using Wayd.Common.Application.Interfaces.ExternalWork;
using Wayd.Common.Application.Validators;

namespace Wayd.Common.Application.Requests.WorkManagement.Commands;

public sealed record SyncExternalWorkItemDependencyChangesCommand(Guid WorkspaceId, List<IExternalWorkItemLink> WorkItemLinks) : ICommand, ILongRunningRequest;

public sealed class SyncExternalWorkItemDependencyChangesCommandValidator : CustomValidator<SyncExternalWorkItemDependencyChangesCommand>
{
    public SyncExternalWorkItemDependencyChangesCommandValidator()
    {
        RuleFor(c => c.WorkspaceId)
            .NotEmpty();

        RuleForEach(c => c.WorkItemLinks)
            .NotNull()
            .SetValidator(new IExternalWorkItemLinkValidator());
    }
}
