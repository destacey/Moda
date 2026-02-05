using Microsoft.AspNetCore.Identity;
using Moda.Common.Domain.Employees;
using NodaTime;

namespace Moda.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
    public string? ObjectId { get; set; }
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public Instant? LastActivityAt { get; set; }
}