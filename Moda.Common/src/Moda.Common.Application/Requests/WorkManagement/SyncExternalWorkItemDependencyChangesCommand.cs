using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Validators;

namespace Moda.Common.Application.Requests.WorkManagement;
public sealed record SyncExternalWorkItemDependencyChangesCommand(Guid WorkspaceId, List<IExternalWorkItemLink> WorkItemLinks) : ICommand;

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
