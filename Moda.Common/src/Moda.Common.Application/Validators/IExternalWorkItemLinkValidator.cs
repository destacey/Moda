using Wayd.Common.Application.Interfaces.ExternalWork;

namespace Wayd.Common.Application.Validators;

public sealed class IExternalWorkItemLinkValidator : CustomValidator<IExternalWorkItemLink>
{
    public IExternalWorkItemLinkValidator()
    {
        RuleFor(c => c.LinkType)
            .NotEmpty();

        RuleFor(c => c.SourceId)
            .GreaterThan(0);

        RuleFor(c => c.TargetId)
            .GreaterThan(0);

        RuleFor(c => c.ChangedDate)
            .NotEmpty();

        RuleFor(c => c.Comment)
            .MaximumLength(1024);

        RuleFor(c => c.ChangedOperation)
            .NotEmpty();

        RuleFor(c => c.SourceWorkspaceId)
            .NotEmpty();

        RuleFor(c => c.TargetWorkspaceId)
            .NotEmpty();
    }
}
