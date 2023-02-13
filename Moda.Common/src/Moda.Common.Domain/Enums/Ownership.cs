using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums;

public enum Ownership
{
    [Display(Description = "The object is owned by Moda.")]
    Owned = 0,

    [Display(Description = "The object is owned by an external system.")]
    Managed = 1,

    [Display(Description = "The object is owned by Moda, but not changable by a user.")]
    System = 2,
}
