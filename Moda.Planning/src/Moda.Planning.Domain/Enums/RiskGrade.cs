using System.ComponentModel.DataAnnotations;

namespace Moda.Planning.Domain.Enums;
public enum RiskGrade
{
    [Display(Name = "Low", Order = 0)]
    Low = 0,

    [Display(Name = "Medium", Order = 1)]
    Medium = 1,

    [Display(Name = "High", Order = 2)]
    High = 2,
}
