using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums.Work;
public enum TeamBoundary
{
    [Display(Name = "Intra-Team", Description = "The dependency is within the same team.", Order = 1)]
    IntraTeam = 1,

    [Display(Name = "Cross-Team", Description = "The dependency is between different teams within the same Team of Teams.", Order = 2)]
    CrossTeam = 2,

    [Display(Name = "Cross-Team of Teams", Description = "The dependency is between different teams across different Team of Teams.", Order = 3)]
    CrossTeamOfTeams = 3,

    [Display(Name = "Unknown", Description = "The relationship between teams is unknown.", Order = 4)]
    Unknown = 4
}
