﻿using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moda.Organization.Application.Interfaces;

namespace Moda.Organization.Application.Validators;
public sealed class ExternalEmployeeValidator : CustomValidator<IExternalEmployee>
{    
    public ExternalEmployeeValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(e => e.Name)
            .NotNull()
            .SetValidator(new PersonNameValidator());

        RuleFor(e => e.EmployeeNumber)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(e => e.Email)
            .NotNull()
            .SetValidator(new EmailAddressValidator());

        RuleFor(e => e.JobTitle)
            .MaximumLength(256);

        RuleFor(e => e.Department)
            .MaximumLength(256);
    }
}
