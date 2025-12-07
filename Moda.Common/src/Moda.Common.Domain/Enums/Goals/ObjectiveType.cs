using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums.Goals;
public enum ObjectiveType
{
    [Display(Name = "Personal", Order = 1)]
    Personal = 1,

    [Display(Name = "Team", Order = 2)]
    Team = 2,

    [Display(Name = "Company", Order = 3)]
    Company = 3,

    [Display(Name = "Planning Interval", Order = 4)]
    PlanningInterval = 4
}
