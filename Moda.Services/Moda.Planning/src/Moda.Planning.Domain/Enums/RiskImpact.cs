using System.ComponentModel.DataAnnotations;

namespace Moda.Planning.Domain.Enums;
public enum RiskImpact
{
    [Display(Name = "Low", Description = "The impact of the risk occurring is low.", Order = 1)]
    Low = 1,

    [Display(Name = "Medium", Description = "The impact of the risk occurring is medium.", Order = 2)]
    Medium = 2,

    [Display(Name = "High", Description = "The impact of the risk occurring is high.", Order = 3)]
    High = 3,
}
