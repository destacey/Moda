using Moda.Common.Application.Interfaces.ExternalWork;

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
            .MaximumLength(256);

        RuleFor(c => c.Created)
            .NotEmpty();

        RuleFor(c => c.CreatedBy)
            .MaximumLength(256);

        RuleFor(c => c.LastModified)
            .NotEmpty();

        RuleFor(c => c.LastModifiedBy)
            .MaximumLength(256);

        RuleFor(c => c.Priority)
            .GreaterThanOrEqualTo(0).When(c => c.Priority.HasValue);

        RuleFor(c => c.StackRank)
            .GreaterThanOrEqualTo(0);

        When(c => c.ActivatedTimestamp.HasValue, () =>
        {
            RuleFor(c => c.ActivatedTimestamp)
                .GreaterThanOrEqualTo(c => c.Created);
        });

        When(c => c.DoneTimestamp.HasValue, () =>
        {
            RuleFor(c => c.DoneTimestamp)
                .GreaterThanOrEqualTo(c => c.Created);
        });

        RuleFor(c => c.ExternalTeamIdentifier)
            .MaximumLength(128);

        When(c => c.StoryPoints.HasValue, () =>
        {
            RuleFor(c => c.StoryPoints)
                .GreaterThanOrEqualTo(0);
        });
    }
}
