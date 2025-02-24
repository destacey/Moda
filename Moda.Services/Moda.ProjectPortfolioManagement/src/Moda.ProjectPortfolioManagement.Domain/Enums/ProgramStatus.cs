using System.ComponentModel.DataAnnotations;

namespace Moda.ProjectPortfolioManagement.Domain.Enums;

/// <summary>
/// Represents the status of a program in its lifecycle.
/// </summary>
public enum ProgramStatus
{
    [Display(Name = "Proposed", Description = "The program is in the initial planning or conceptualization stage. It has not yet been approved or activated.", Order = 1)]
    Proposed = 1,

    [Display(Name = "Active", Description = "The program has been approved and is actively managed, with projects in progress.", Order = 2)]
    Active = 2,

    [Display(Name = "Completed", Description = "The program has achieved its goals and is considered finished.", Order = 3)]
    Completed = 3,

    [Display(Name = "Cancelled", Description = "The program was terminated before achieving its goals.", Order = 4)]
    Cancelled = 4
}
