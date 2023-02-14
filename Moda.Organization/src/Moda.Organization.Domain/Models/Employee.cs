using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Extensions;
using NodaTime;

namespace Moda.Organization.Domain.Models;
public sealed class Employee : BaseAuditableEntity<Guid>, IActivatable
{
    private readonly List<Employee> _directReports = new();
    private PersonName _name = null!;
    private string _employeeNumber = null!;
    private EmailAddress _email = null!;
    private string? _jobTitle;
    private string? _department;
    private string? _officeLocation;
    private Guid? _managerId;

    private Employee() { }

    private Employee(Guid personId, PersonName personName, string employeeNumber, Instant? hireDate, EmailAddress email, string? jobTitle, string? department, string? officeLocation, Guid? managerId)
    {
        Id = Guard.Against.Default(personId);
        Name = personName;
        EmployeeNumber = employeeNumber;
        HireDate = hireDate;
        Email = email;
        JobTitle = jobTitle;
        Department = department;
        OfficeLocation = officeLocation;
        ManagerId = managerId;
    }

    /// <summary>Gets the local identifier.</summary>
    /// <value>The local identifier.</value>
    public int LocalId { get; private set; }

    /// <summary>Gets the employee name.</summary>
    /// <value>The employee name.</value>
    public PersonName Name
    {
        get => _name;
        private set => _name = Guard.Against.Null(value, nameof(EmployeeNumber));
    }

    /// <summary>Gets the employee number.</summary>
    /// <value>The employee number.</value>
    public string EmployeeNumber
    {
        get => _employeeNumber;
        private set => _employeeNumber = Guard.Against.NullOrWhiteSpace(value, nameof(EmployeeNumber)).Trim();
    }

    /// <summary>Gets the hire date.</summary>
    /// <value>The hire date.</value>
    public Instant? HireDate { get; private set; }

    /// <summary>Gets the email.</summary>
    /// <value>The email.</value>
    public EmailAddress Email
    {
        get => _email;
        private set => _email = Guard.Against.Null(value, nameof(Email));
    }

    /// <summary>Gets the job title.</summary>
    /// <value>The job title.</value>
    public string? JobTitle { get => _jobTitle; private set => _jobTitle = value.NullIfWhiteSpacePlusTrim(); }

    /// <summary>Gets the department.</summary>
    /// <value>The department.</value>
    public string? Department { get => _department; private set => _department = value.NullIfWhiteSpacePlusTrim(); }

    /// <summary>Gets the office location.</summary>
    /// <value>The office location.</value>
    public string? OfficeLocation { get => _officeLocation; private set => _officeLocation = value.NullIfWhiteSpacePlusTrim(); }

    /// <summary>Gets the manager identifier.</summary>
    /// <value>The manager identifier.</value>
    public Guid? ManagerId 
    { 
        get => _managerId; 
        private set => _managerId = value.HasValue ? Guard.Against.Default(value) : null; 
    }

    /// <summary>Gets the manager.</summary>
    /// <value>The manager.</value>
    public Employee? Manager { get; private set; }

    /// <summary>Gets the direct reports.</summary>
    /// <value>The employee's direct reports.</value>
    public IReadOnlyCollection<Employee> DirectReports => _directReports.AsReadOnly();

    /// <summary>
    /// Indicates whether the employee is active or not.  
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// The process for activating an employee.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Activate(Instant timestamp)
    {
        if (!IsActive)
        {
            // TODO is there logic that would prevent activation?
            IsActive = true;
            AddDomainEvent(EntityActivatedEvent.WithEntity(this, timestamp));
        }

        return Result.Success();
    }

    /// <summary>
    /// The process for deactivating an employee.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Deactivate(Instant timestamp)
    {
        if (IsActive)
        {
            // TODO is there logic that would prevent deactivation?
            IsActive = false;
            AddDomainEvent(EntityDeactivatedEvent.WithEntity(this, timestamp));
        }

        return Result.Success();
    }

    /// <summary>Updates the current employee.</summary>
    /// <param name="name">The employee's name.</param>
    /// <param name="employeeNumber">The employee number.</param>
    /// <param name="hireDate">The hire date.</param>
    /// <param name="email">The email.</param>
    /// <param name="jobTitle">The job title.</param>
    /// <param name="department">The department.</param>
    /// <param name="officeLocation">The office location.</param>
    /// <param name="managerId">The manager identifier.</param>
    /// <param name="isActive">if set to <c>true</c> [is active].</param>
    /// <param name="timestamp">The timestamp for the event.</param>
    /// <returns>Result</returns>
    public Result Update(
        PersonName name,
        string employeeNumber,
        Instant? hireDate,
        EmailAddress email,
        string? jobTitle,
        string? department,
        string? officeLocation,
        Guid? managerId,
        bool isActive,
        Instant timestamp
        )
    {
        try
        {
            if (Name != name) Name = name;
            if (Email != email) Email = email;

            EmployeeNumber = employeeNumber;
            HireDate = hireDate;
            JobTitle = jobTitle;
            Department = department;
            OfficeLocation = officeLocation;

            if (ManagerId != managerId)
            {
                ManagerId = managerId;
                Manager = null;
            }

            if (IsActive != isActive)
            {
                var result = IsActive ? Activate(timestamp) : Deactivate(timestamp);
                if (result.IsFailure)
                {
                    return Result.Failure(result.Error);
                }
            }

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    /// <summary>Updates the manager identifier.</summary>
    /// <param name="managerId">The manager identifier.</param>
    public void UpdateManagerId(Guid? managerId, Instant timestamp)
    {
        ManagerId = managerId;
        AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));
    }

    /// <summary>
    /// Creates an Employee and adds a domain event with the timestamp.
    /// </summary>
    /// <param name="personId">The person identifier.</param>
    /// <param name="personName">Name of the person.</param>
    /// <param name="employeeNumber">The employee identifier.</param>
    /// <param name="hireDate">The hire date.</param>
    /// <param name="email">The email.</param>
    /// <param name="jobTitle">The job title.</param>
    /// <param name="department">The department.</param>
    /// <param name="officeLocation">The office location.</param>
    /// <param name="managerId">The manager identifier.</param>
    /// <param name="timestamp">The timestamp for the domain event.</param>
    /// <returns>An Employee</returns>
    public static Employee Create(
        Guid personId,
        PersonName personName,
        string employeeNumber,
        Instant? hireDate,
        EmailAddress email,
        string? jobTitle,
        string? department,
        string? officeLocation,
        Guid? managerId,
        Instant timestamp)
    {
        Employee employee = new(personId, personName, employeeNumber, hireDate, email, jobTitle, department, officeLocation, managerId);
        employee.AddDomainEvent(EntityCreatedEvent.WithEntity(employee, timestamp));
        return employee;
    }
}
