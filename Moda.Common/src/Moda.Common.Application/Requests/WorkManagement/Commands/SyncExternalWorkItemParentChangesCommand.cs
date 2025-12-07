using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Validators;

namespace Moda.Common.Application.Requests.WorkManagement.Commands;
public sealed record SyncExternalWorkItemParentChangesCommand(Guid WorkspaceId, List<IExternalWorkItemLink> WorkItemLinks) : ICommand, ILongRunningRequest;

public sealed class SyncExternalWorkItemParentChangesCommandValidator : CustomValidator<SyncExternalWorkItemParentChangesCommand>
{
    public SyncExternalWorkItemParentChangesCommandValidator()
    {
        RuleFor(c => c.WorkspaceId)
            .NotEmpty();

        RuleForEach(c => c.WorkItemLinks)
            .NotNull()
            .SetValidator(new IExternalWorkItemLinkValidator());
    }
}
