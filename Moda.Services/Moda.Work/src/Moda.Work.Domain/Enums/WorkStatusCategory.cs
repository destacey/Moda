using System.ComponentModel.DataAnnotations;

namespace Moda.Work.Domain.Enums;

// Max length of 32 characters
public enum WorkStatusCategory
{
    [Display(Name = "Proposed", Description = "The work has been proposed but not yet started.", Order = 1)]
    Proposed = 0,

    [Display(Name = "Active", Description = "The work is currently being performed.", Order = 2)]
    Active = 1,

    [Display(Name = "Done", Description = "The work has been completed.", Order = 3)]
    Done = 2,

    [Display(Name = "Removed", Description = "The work has been removed from the backlog without being completed.", Order = 4)]
    Removed = 3
}
