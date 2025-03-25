using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Models.KeyPerformanceIndicators;

// max length of 32 characters

public enum KpiTargetDirection
{
    [Display(Name = "Increase", Description = "Indicates that the KPI target is to increase the measured value.", Order = 1)]
    Increase = 1,

    [Display(Name = "Decrease", Description = "Indicates that the KPI target is to decrease the measured value.", Order = 2)]
    Decrease = 2
}
