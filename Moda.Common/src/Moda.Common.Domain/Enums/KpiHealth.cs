using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums;

public enum KpiHealth
{
    [Display(Name = "Healthy", Description = "Indicates that the KPI measurement has met or exceeded the target value.", Order = 1)]
    Healthy = 1,

    [Display(Name = "At Risk", Description = "Indicates that the KPI measurement is between the at-risk value and the target value.", Order = 2)]
    AtRisk = 2,

    [Display(Name = "Unhealthy", Description = "Indicates that the KPI measurement has not met the at-risk value, or the target value when no at-risk value is set.", Order = 3)]
    Unhealthy = 3
}
