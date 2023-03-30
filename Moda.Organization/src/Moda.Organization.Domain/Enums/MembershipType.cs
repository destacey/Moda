using System.ComponentModel.DataAnnotations;

namespace Moda.Organization.Domain.Enums;
public enum MembershipType
{
    [Display(Name = "Team to Team", Description = "A membership that connects a team to a parent team.", Order = 1)]
    TeamToTeam = 0,

    [Display(Name = "Employee to Team", Description = "A membership that connects an employee to a team.", Order = 2)]
    EmployeeToTeam = 1,
}
