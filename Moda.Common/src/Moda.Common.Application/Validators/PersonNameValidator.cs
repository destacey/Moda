using FluentValidation;
using Moda.Common.Models;

namespace Moda.Common.Application.Validators;
public sealed class PersonNameValidator : CustomValidator<PersonName>
{
    public PersonNameValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(p => p.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(p => p.MiddleName)
            .MaximumLength(100);

        RuleFor(p => p.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(p => p.Suffix)
            .MaximumLength(50);

        RuleFor(p => p.Title)
            .MaximumLength(50);
    }
}
