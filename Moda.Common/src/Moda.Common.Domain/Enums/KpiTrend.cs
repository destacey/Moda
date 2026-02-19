using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums;

public enum KpiTrend
{
    [Display(Name = "No Data", Description = "Indicates that there is no previous data point to compare against for determining the trend.", Order = 1)]
    NoData = 1,
    [Display(Name = "Improving", Description = "Indicates that the KPI is showing positive progress.", Order = 2)]
    Improving = 2,
    [Display(Name = "Worsening", Description = "Indicates that the KPI is showing negative progress.", Order = 3)]
    Worsening = 3,
    [Display(Name = "Stable", Description = "Indicates that the KPI is showing no significant change.", Order = 4)]
    Stable = 4
}
