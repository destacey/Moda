using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums.Work;

// Max length of 32 characters
public enum WorkTypeTier
{
    [Display(Name = "Portfolio", Description = "Portfolio tiers provide a way to group work types into a more granular hierarchical structure.  These work types are containers for lower level work types.  This is the only hierarchy category that allows multiple levels.", Order = 1)]
    Portfolio = 0,

    [Display(Name = "Requirement", Description = "The requirement tier contains your base level work types.  These work types represent the work being done by a team.", Order = 2)]
    Requirement = 1,

    [Display(Name = "Task", Description = "The task tier contains work types that are owned and managed by a parent work type.  The parent work type is typically from the requirement tier.", Order = 3)]
    Task = 2,

    [Display(Name = "Other", Description = "A tier for non-standard work types.  Work Types in this tier will not appear in backlog or iteration views.  It is used for special work types.  This is also the default tier for new work types.", Order = 4)]
    Other = 3
}
