using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums.Work;
public enum DependencyStatus
{

    [Display(Name = "ToDo", Description = "The dependency has been identified but not yet started.", Order = 1)]
    Proposed = 1,

    [Display(Name = "Active", Description = "The dependency is currently being worked on.", Order = 2)]
    Active = 2,

    [Display(Name = "Done", Description = "The dependency has been completed.", Order = 3)]
    Done = 3,
}
