using System.ComponentModel.DataAnnotations;

namespace Moda.Planning.Domain.Enums;

// max length of 32 characters
public enum IterationType
{
    [Display(Name = "Development", Description = "Development iteration/sprint.", Order = 1)]
    Development = 1,

    [Display(Name = "IP", Description = "Innovation and planning iteration/sprint.", Order = 2)]
    InnovationAndPlanning = 2,
}