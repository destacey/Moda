using System.ComponentModel.DataAnnotations;

namespace Moda.Planning.Domain.Enums;
public enum RiskCategory
{
    [Display(Name = "Resolved", Description = "The risk is determined to not be a threat at this time. No further action is required.", Order = 0)]
    Resolved = 0,

    [Display(Name = "Owned", Description = "The risk cannot be resolved within the meeting, so a member of the team is selected to ‘own’ the handling of that risk. This person is responsible for making sure that this risk is appropriately managed.", Order = 1)]
    Owned = 1,

    [Display(Name = "Accepted", Description = "The risk cannot be resolved, so it must be accepted as-is and dealt with as necessary.", Order = 2)]
    Accepted = 2,

    [Display(Name = "Mitigated", Description = "Actions were taken to mitigate the risk.", Order = 3)]
    Mitigated = 3,
}
