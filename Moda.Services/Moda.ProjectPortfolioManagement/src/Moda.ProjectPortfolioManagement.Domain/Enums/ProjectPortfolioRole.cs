using System.ComponentModel.DataAnnotations;

namespace Moda.ProjectPortfolioManagement.Domain.Enums;

public enum ProjectPortfolioRole
{
    [Display(Name = "Portfolio Sponsor", Description = "Provides executive oversight, funding, and strategic direction for the portfolio.", Order = 1)]
    Sponsor = 1,

    [Display(Name = "Portfolio Owner", Description = "Accountable for the overall success and performance of the portfolio.", Order = 2)]
    Owner = 2,

    [Display(Name = "Portfolio Manager", Description = "Manages portfolio governance, prioritization, and alignment with business strategy.", Order = 3)]
    Manager = 3,
}
