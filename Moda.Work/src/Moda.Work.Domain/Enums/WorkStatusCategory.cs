using System.ComponentModel.DataAnnotations;

namespace Moda.Work.Domain.Enums;

public enum WorkStatusCategory
{
    [Display(Name = "To Do")]
    ToDo = 0,

    [Display(Name = "In Progress")]
    InProgress = 1,

    [Display(Name = "Done")]
    Done = 2
}
