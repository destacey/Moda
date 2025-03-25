using System.ComponentModel.DataAnnotations;

namespace Moda.ProjectPortfolioManagement.Domain.Enums;

public enum StrategicInitiativeRole
{
    [Display(Name = "Strategic Initiative Sponsor", Description = "Provides executive oversight, funding, and strategic direction for the strategic initiative.", Order = 1)]
    Sponsor = 1,

    [Display(Name = "Strategic Initiative Owner", Description = "Accountable for the overall success and performance of the strategic initiative.", Order = 2)]
    Owner = 2,
}
