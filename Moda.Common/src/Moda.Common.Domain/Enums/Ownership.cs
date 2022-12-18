using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums;

public enum Ownership
{
    [Display(Description = "The object is owned by Moda.")]
    Owned = 0,

    [Display(Description = "The object is owned by an external system.")]
    Managed = 1
}
