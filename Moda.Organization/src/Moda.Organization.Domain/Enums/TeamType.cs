using System.ComponentModel.DataAnnotations;

namespace Moda.Organization.Domain.Enums;

public enum TeamType
{
    [Display(Name = "Team", Description = "A team is a collection of team members that work together to execute against a prioritized set of goals.", Order = 1)]
    Team = 0,

    [Display(Name = "Team of Teams", Description = "A team of teams is a collection of teams and/or other team of teams that aims to help deliver products collaboratively in the same complex environment.", Order = 2)]
    TeamOfTeams = 1,
}
