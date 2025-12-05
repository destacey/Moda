using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums.Work;

public enum DependencyScope
{
    [Display(Name = "Unknown", Description = "The scope of the dependency is unknown or not specified.", Order = 0)]
    Unknown = 0,

    [Display(Name = "Intra-Team", Description = "A dependency between two work items that are owned and executed by the same team.", Order = 1)]
    IntraTeam = 1,

    [Display(Name = "Cross-Team", Description = "A dependency between two work items that are owned and executed by different teams.", Order = 2)]
    CrossTeam = 2,
}
