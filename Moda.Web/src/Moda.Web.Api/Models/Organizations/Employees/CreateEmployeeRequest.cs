using FluentValidation;
using NodaTime;

namespace Moda.Web.Api.Models.Organizations.Employees;

public sealed record CreateEmployeeRequest
{
    /// <summary>Gets the first name.</summary>
    /// <value>The first name.</value>
    public string FirstName { get; set; } = null!;

    /// <summary>Gets the middle name.</summary>
    /// <value>The middle name.</value>
    public string? MiddleName { get; set; }

    /// <summary>Gets the last name.</summary>
    /// <value>The last name.</value>
    public string LastName { get; set; } = null!;

    /// <summary>Gets the suffix.</summary>
    /// <value>The suffix.</value>
    public string? Suffix { get; set; }

    /// <summary>Gets the employee's personal title.</summary>
    /// <value>The title.</value>
    public string? Title { get; set; }

    /// <summary>Gets the employee number.</summary>
    /// <value>The employee number.</value>
    public string EmployeeNumber { get; set; } = null!;

    /// <summary>Gets the hire date.</summary>
    /// <value>The hire date.</value>
    public LocalDate? HireDate { get; set; }

    /// <summary>Gets the email.</summary>
    /// <value>The email.</value>
    public string Email { get; set; } = null!;

    /// <summary>Gets the job title.</summary>
    /// <value>The job title.</value>
    public string? JobTitle { get; set; }

    /// <summary>Gets the department.</summary>
    /// <value>The department.</value>
    public string? Department { get; set; }

    /// <summary>Gets the manager identifier.</summary>
    /// <value>The manager identifier.</value>
    public Guid? ManagerId { get; set; }

    public CreateEmployeeCommand ToCreateEmployeeCommand()
    {
        PersonName personName = new(FirstName, MiddleName, LastName, Suffix, Title);
        EmailAddress emailAddress = new(Email);

        return new CreateEmployeeCommand(personName, EmployeeNumber, HireDate, emailAddress, JobTitle, Department, ManagerId);
    }
}

public sealed class CreateEmployeeRequestValidator : CustomValidator<CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator()
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

        RuleFor(e => e.EmployeeNumber)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(e => e.Email)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(e => e.JobTitle)
            .MaximumLength(256);

        RuleFor(e => e.Department)
            .MaximumLength(256);
    }
}
