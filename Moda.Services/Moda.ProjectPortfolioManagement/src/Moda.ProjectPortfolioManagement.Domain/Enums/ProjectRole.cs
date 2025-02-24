using System.ComponentModel.DataAnnotations;

namespace Moda.ProjectPortfolioManagement.Domain.Enums;

// max length of 32 characters

public enum ProjectRole
{
    [Display(Name = "Project Sponsor", Description = "Provides overall guidance, support, and funding for the project.", Order = 1)]
    Sponsor = 1,

    [Display(Name = "Project Owner", Description = "Accountable for the project's outcomes and ensures alignment with business goals.", Order = 2)]
    Owner = 2,

    [Display(Name = "Project Manager", Description = "Responsible for planning, execution, and successful delivery of the project.", Order = 3)]
    Manager = 3,
}
