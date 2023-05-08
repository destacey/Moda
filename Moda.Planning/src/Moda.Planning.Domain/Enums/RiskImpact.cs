using System.ComponentModel.DataAnnotations;

namespace Moda.Planning.Domain.Enums;
public enum RiskImpact
{
    [Display(Name = "Low", Description = "The impact of the risk occurring is low.", Order = 0)]
    Low = 0,

    [Display(Name = "Medium", Description = "The impact of the risk occurring is medium.", Order = 1)]
    Medium = 1,

    [Display(Name = "High", Description = "The impact of the risk occurring is high.", Order = 2)]
    High = 2,
}
