using System.ComponentModel.DataAnnotations;

namespace Moda.Organization.Domain.Enums;
public enum MembershipState
{
    [Display(Name = "Past", Description = "The membership was completed and is in the past.", Order = 1)]
    Past = 0,

    [Display(Name = "Active", Description = "The membership is currently active.", Order = 2)]
    Active = 1,

    [Display(Name = "Future", Description = "The membership hasn't started and is in the future.", Order = 3)]
    Future = 2,
}
