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

        RuleFor(c => c.IsActive)
            .NotEmpty();

        RuleFor(c => c.ChangedOperation)
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(c => c.SourceProjectId)
            .NotEmpty();

        RuleFor(c => c.TargetProjectId)
            .NotEmpty();
    }
}
