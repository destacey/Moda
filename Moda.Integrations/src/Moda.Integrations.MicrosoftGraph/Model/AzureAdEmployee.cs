using Microsoft.Graph;
using Moda.Common.Models;
using Moda.Organization.Application.Interfaces;
using NodaTime;

namespace Moda.Integrations.MicrosoftGraph.Model;
public sealed record AzureAdEmployee : IExternalEmployee
{
    public AzureAdEmployee(User user)
    {
        EmployeeNumber = user.Id;
        Name = new PersonName(user.GivenName, null, user.Surname);
        HireDate = user.HireDate is not null
            ? Instant.FromDateTimeOffset((DateTimeOffset)user.HireDate)
            : user.EmployeeHireDate is not null
                ? Instant.FromDateTimeOffset((DateTimeOffset)user.EmployeeHireDate)
                : null;
        Email = new Common.Models.EmailAddress(user.Mail ?? user.UserPrincipalName);
        JobTitle = user.JobTitle;
        Department = user.Department;
        OfficeLocation = user.OfficeLocation;
        ManagerEmployeeNumber = user.Manager?.Id;
        IsActive = user.AccountEnabled ?? false;
    }

    public string EmployeeNumber { get; set; }
    public PersonName Name { get; set; }
    public Instant? HireDate { get; set; }
    public Common.Models.EmailAddress Email { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? OfficeLocation { get; set; }
    public string? ManagerEmployeeNumber { get; set; }
    public bool IsActive { get; set; }
}
