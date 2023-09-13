using System.ComponentModel.DataAnnotations;

namespace Moda.Planning.Domain.Enums;
public enum RiskGrade
{
    [Display(Name = "Low", Order = 1)]
    Low = 1,

    [Display(Name = "Medium", Order = 2)]
    Medium = 2,

    [Display(Name = "High", Order = 3)]
    High = 3,
}
