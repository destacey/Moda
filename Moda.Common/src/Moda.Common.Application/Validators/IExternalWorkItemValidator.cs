using Moda.Common.Application.Interfaces.Work;

namespace Moda.Common.Application.Validators;
public sealed class IExternalWorkItemValidator : CustomValidator<IExternalWorkItem>
{
    public IExternalWorkItemValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty();

        RuleFor(c => c.Title)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(c => c.WorkType)
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(c => c.WorkStatus)
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(c => c.ParentId)
            .GreaterThan(0).When(c => c.ParentId.HasValue);

        RuleFor(c => c.AssignedTo)
            .MaximumLength(64);

        RuleFor(c => c.Created)
            .NotEmpty();

        RuleFor(c => c.CreatedBy)
            .MaximumLength(64);

        RuleFor(c => c.LastModified)
            .NotEmpty();

        RuleFor(c => c.LastModifiedBy)
            .MaximumLength(64);

        RuleFor(c => c.Priority)
            .GreaterThanOrEqualTo(0).When(c => c.Priority.HasValue);

        RuleFor(c => c.StackRank)
            .GreaterThanOrEqualTo(0);

        RuleFor(c => c.ExternalTeamIdentifier)
            .MaximumLength(128);
    }
}
