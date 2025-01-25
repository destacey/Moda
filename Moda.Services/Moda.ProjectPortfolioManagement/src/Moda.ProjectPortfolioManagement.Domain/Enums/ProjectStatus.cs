using System.ComponentModel.DataAnnotations;

namespace Moda.ProjectPortfolioManagement.Domain.Enums;

// max length of 32 characters

/// <summary>
/// Represents the various statuses a project can have during its lifecycle.
/// </summary>
public enum ProjectStatus
{
    [Display(Name = "Proposed", Description = "The project is in the initial planning and evaluation stage.", Order = 1)]
    Proposed = 1,

    [Display(Name = "Active", Description = "The project has been approved and is currently in progress.", Order = 2)]
    Active = 2,

    [Display(Name = "Completed", Description = "The project has been successfully completed and closed.", Order = 3)]
    Completed = 3,

    [Display(Name = "Cancelled", Description = "The project has been terminated before completion.", Order = 4)]
    Cancelled = 4
}
