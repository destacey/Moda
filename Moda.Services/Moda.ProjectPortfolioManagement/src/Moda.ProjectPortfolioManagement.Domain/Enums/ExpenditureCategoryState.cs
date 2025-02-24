using System.ComponentModel.DataAnnotations;

namespace Moda.ProjectPortfolioManagement.Domain.Enums;

// max length of 32 characters

public enum ExpenditureCategoryState
{

    [Display(Name = "Proposed", Description = "The expenditure category is defined but not yet approved for use.", Order = 1)]
    Proposed = 1,

    [Display(Name = "Active", Description = "The expenditure category is actively used in expense classification and reporting.", Order = 2)]
    Active = 2,

    [Display(Name = "Archived", Description = "The expenditure category is no longer used for new expenses but remains for historical reference.", Order = 3)]
    Archived = 3
}
