using CSharpFunctionalExtensions;
using NodaTime;

namespace Moda.Organization.Domain.Models;
public sealed class Employee : BaseAuditableEntity<Guid>, IAggregateRoot, IActivatable
{
    private readonly List<Employee> _directReports = new();

    private Employee() { }

    public Employee(Guid personId, PersonName name, string? employeeId, LocalDate? hireDate, string email, string? jobTitle, string? department, Guid? managerId)
    {
        Id = personId;
        Name = name;
        EmployeeId = employeeId;
        HireDate = hireDate;
        Email = new EmailAddress(email);
        JobTitle = jobTitle;
        Department = department;
        ManagerId = managerId;
    }

    public PersonName Name { get; set; } = null!;
    public string? EmployeeId { get; set; }
    public LocalDate? HireDate { get; set; }
    public EmailAddress Email { get; set; } = null!;
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public Guid? ManagerId { get; set; }
    public Employee? Manager { get; set; }

    public IReadOnlyCollection<Employee> DirectReports => _directReports.AsReadOnly();

    /// <summary>
    /// Indicates whether the employee is active or not.  
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// The process for activating an employee.
    /// </summary>
    /// <param name="activatedOn"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Activate(Instant activatedOn)
    {
        if (!IsActive)
        {
            // TODO is there logic that would prevent activation?
            IsActive = true;
            AddDomainEvent(EntityActivatedEvent.WithEntity(this, activatedOn));
        }

        return Result.Success();
    }

    /// <summary>
    /// The process for deactivating an employee.
    /// </summary>
    /// <param name="deactivatedOn"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Deactivate(Instant deactivatedOn)
    {
        if (IsActive)
        {
            // TODO is there logic that would prevent deactivation?
            IsActive = false;
            AddDomainEvent(EntityDeactivatedEvent.WithEntity(this, deactivatedOn));
        }

        return Result.Success();
    }
}
