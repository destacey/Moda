using NodaTime;

namespace Moda.Organization.Application.Interfaces;
public interface IExternalEmployee
{
    string EmployeeNumber { get; }
    PersonName Name { get; }
    LocalDate? HireDate { get; }
    EmailAddress Email { get; }
    string? JobTitle { get; }
    string? Department { get; }
    string? ManagerEmployeeNumber { get; }
    bool IsActive { get; }
}
