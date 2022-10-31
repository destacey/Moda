using NodaTime;

namespace Moda.Organization.Application.Employees.Dtos;
public sealed record EmployeeDetailsDto
{
    /// <summary>Gets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; }

    /// <summary>Gets the first name.</summary>
    /// <value>The first name.</value>
    public string FirstName { get; } = null!;

    /// <summary>Gets the middle name.</summary>
    /// <value>The middle name.</value>
    public string? MiddleName { get; }

    /// <summary>Gets the last name.</summary>
    /// <value>The last name.</value>
    public string LastName { get; } = null!;

    /// <summary>Gets the suffix.</summary>
    /// <value>The suffix.</value>
    public string? Suffix { get; }

    /// <summary>Gets the employee's personal title.</summary>
    /// <value>The title.</value>
    public string? Title { get; }

    /// <summary>Gets the employee number.</summary>
    /// <value>The employee number.</value>
    public string EmployeeNumber { get; } = null!;

    /// <summary>Gets the hire date.</summary>
    /// <value>The hire date.</value>
    public LocalDate? HireDate { get; private set; }

    /// <summary>Gets the email.</summary>
    /// <value>The email.</value>
    public string Email { get; } = null!;

    /// <summary>Gets the job title.</summary>
    /// <value>The job title.</value>
    public string? JobTitle { get; }

    /// <summary>Gets the department.</summary>
    /// <value>The department.</value>
    public string? Department { get; }

    /// <summary>Gets the manager identifier.</summary>
    /// <value>The manager identifier.</value>
    public Guid? ManagerId { get; }

    /// <summary>Gets the manager.</summary>
    /// <value>The manager.</value>
    public string? ManagerName { get; }

    /// <summary>
    /// Indicates whether the employee is active or not.  
    /// </summary>
    public bool IsActive { get; }
}
