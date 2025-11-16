using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums.Work;

// max length of 32 characters
public enum DependencyState
{
    [Display(Name = "To Do", Description = "The dependency has been identified but not yet started.", Order = 1)]
    ToDo = 1,

    [Display(Name = "In Progress", Description = "The dependency is currently being worked on.", Order = 2)]
    InProgress = 2,

    [Display(Name = "Done", Description = "The dependency has been completed or removed.", Order = 3)]
    Done = 3,

    [Display(Name = "Removed", Description = "The dependency has been removed from consideration.", Order = 4)]
    Removed = 4,

    [Display(Name = "Deleted", Description = "The dependency has been deleted.", Order = 5)]
    Deleted = 5
}
