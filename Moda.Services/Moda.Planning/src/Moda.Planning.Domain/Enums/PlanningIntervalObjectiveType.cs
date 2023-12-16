using System.ComponentModel.DataAnnotations;

namespace Moda.Planning.Domain.Enums;
public enum PlanningIntervalObjectiveType
{
    [Display(Name = "Team", Order = 1)]
    Team = 1,
    [Display(Name = "Team of Teams", Order = 2)]
    TeamOfTeams = 2,
}
