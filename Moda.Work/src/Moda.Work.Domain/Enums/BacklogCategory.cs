using System.ComponentModel.DataAnnotations;

namespace Moda.Work.Domain.Enums;
public enum BacklogCategory
{
    [Display(Name = "Portfolio", Description = "Portfolio backlogs provide a way to group related items into a hierarchical structure.  This is the only backlog category that allows multiple backlog levels.", Order = 1)]
    Portfolio = 0,

    [Display(Name = "Requirement", Description = "The requirement backlog category contains your base level work items.  These items are owned by a single team.", Order = 2)]
    Requirement = 1,

    [Display(Name = "Task", Description = "The task backlog contains task work items that are owned and managed by a parent work item.  The parent work item is typically from the requirement backlog category.", Order = 3)]
    Task = 2,

    [Display(Name = "Issue", Description = "A problem or blocker that needs to be resolved.  Issues are a way to document a problem and do not represent the work to resolve it.  They are typically linked to other work items that represent the action taken.", Order = 4)]
    Issue = 3
}
