using System.ComponentModel.DataAnnotations;

namespace Wayd.Common.Domain.Enums;

// Max length of 32 characters
public enum Ownership
{
    [Display(Description = "The object is owned by Wayd.")]
    Owned = 0,

    [Display(Description = "The object is owned by an external system.")]
    Managed = 1,

    [Display(Description = "The object is owned by Wayd, but not changeable by a user.")]
    System = 2,
}
