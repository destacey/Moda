using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums.Work;
public enum DependencyStatus
{
    [Display(Name = "To Do", Description = "The dependency has been identified but not yet started.", Order = 1)]
    ToDo = 1,

    [Display(Name = "In Progress", Description = "The dependency is currently being worked on.", Order = 2)]
    InProgress = 2,

    [Display(Name = "Done", Description = "The dependency has been completed.", Order = 3)]
    Done = 3,
}
