namespace Moda.Common.Application.Validators;
public sealed class AzdoIterationMetadataValidator : CustomValidator<AzdoIterationMetadata>
{
    public AzdoIterationMetadataValidator()
    {
        RuleFor(c => c.ProjectId)
            .NotEmpty();

        RuleFor(c => c.Identifier)
            .NotEmpty();

        RuleFor(c => c.Path)
            .NotEmpty()
            .MaximumLength(4000);
    }
}
