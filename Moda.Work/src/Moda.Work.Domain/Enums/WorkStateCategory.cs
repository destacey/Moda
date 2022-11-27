using System.ComponentModel.DataAnnotations;

namespace Moda.Work.Domain.Enums;

public enum WorkStateCategory
{
    [Display(Name = "Proposed", Description = "The work has been proposed but not yet started.")]
    Proposed = 0,

    [Display(Name = "Active", Description = "The work is currently being performed.")]
    Active = 1,

    [Display(Name = "Done", Description = "The work has been completed.")]
    Done = 2,

    [Display(Name = "Removed", Description = "The work has been removed from the backlog without being completed.")]
    Removed = 3
}
