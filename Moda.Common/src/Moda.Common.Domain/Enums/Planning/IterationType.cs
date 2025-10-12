using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums.Planning;
public enum IterationType
{
    [Display(Name = "Iteration", Description = "A time-boxed period of work.", Order = 1)]
    Iteration = 1,

    [Display(Name = "Sprint", Description = "A time-boxed period of work, typically used in Agile methodologies.", Order = 2)]
    Sprint = 2,
}
