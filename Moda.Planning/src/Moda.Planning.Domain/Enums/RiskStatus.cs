using System.ComponentModel.DataAnnotations;

namespace Moda.Planning.Domain.Enums;
public enum RiskStatus
{
    [Display(Name = "Open", Description = "The risk is open.", Order = 0)]
    Open = 0,

    [Display(Name = "Closed", Description = "The risk is closed and is no longer being managed.", Order = 1)]
    Closed = 1
}
