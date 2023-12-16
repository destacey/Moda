using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums;
public enum SystemContext
{
    // Current Db length is set to 64 characters
    // Template: [Service].[Domain?].[Object]

    // 1000 - 1999: Common

    // 2000 - 2999: Planning
    [Display(Name = "Planning.PlanningInterval.Objective", Description = "Planning Interval Objective", Order = 1)]
    PlanningPlanningIntervalObjective = 2010,
}
