using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Models.KeyPerformanceIndicators;

/// <summary>
/// Defines the units of measurement for KPIs.
/// </summary>
public enum KpiUnit
{
    [Display(Name = "Percentage", Description = "Represents a percentage value.", Order = 1)]
    Percentage = 1,

    [Display(Name = "Number", Description = "Represents a plain numeric value.", Order = 2)]
    Number = 2,

    [Display(Name = "USD", Description = "Represents a monetary value in US dollars.", Order = 3)]
    USD = 3
}
