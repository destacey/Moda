using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Common.Application.Validators;
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

        RuleFor(c => c.ChangedOperation)
            .NotEmpty();

        RuleFor(c => c.SourceProjectId)
            .NotEmpty();

        RuleFor(c => c.TargetProjectId)
            .NotEmpty();
    }
}
