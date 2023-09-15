using System.ComponentModel.DataAnnotations;

namespace Moda.Planning.Domain.Enums;
public enum RiskStatus
{
    [Display(Name = "Open", Description = "The risk is open.", Order = 1)]
    Open = 1,

    [Display(Name = "Closed", Description = "The risk is closed and is no longer being managed.", Order = 2)]
    Closed = 2
}
