using System.ComponentModel.DataAnnotations;

namespace Moda.Planning.Domain.Enums;
public enum IterationState
{
    [Display(Name = "Completed", Description = "The iteration was completed and is in the past.", Order = 3)]
    Completed = 1,

    [Display(Name = "Active", Description = "The iteration is currently active.", Order = 1)]
    Active = 2,

    [Display(Name = "Future", Description = "The iteration hasn't started and is in the future.", Order = 2)]
    Future = 3,
}
