using System.ComponentModel.DataAnnotations;

namespace Moda.ProjectPortfolioManagement.Domain.Enums;

public enum ProgramRole
{
    [Display(Name = "Program Sponsor", Description = "Ensures the program aligns with strategic objectives and secures necessary funding.", Order = 1)]
    Sponsor = 1,

    [Display(Name = "Program Owner", Description = "Accountable for program-level success and stakeholder engagement.", Order = 2)]
    Owner = 2,

    [Display(Name = "Program Manager", Description = "Oversees execution, resource allocation, and coordination across projects within the program.", Order = 3)]
    Manager = 3,
}