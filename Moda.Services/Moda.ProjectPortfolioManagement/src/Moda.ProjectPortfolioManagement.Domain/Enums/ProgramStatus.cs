using System.ComponentModel.DataAnnotations;
using Moda.Common.Domain.Enums;

namespace Moda.ProjectPortfolioManagement.Domain.Enums;

/// <summary>
/// Represents the status of a program in its lifecycle.
/// </summary>
public enum ProgramStatus
{
    [Display(Name = "Proposed", Description = "The program is in the initial planning or conceptualization stage. It has not yet been approved or activated.", Order = 1, GroupName = nameof(LifecyclePhase.NotStarted))]
    Proposed = 1,

    [Display(Name = "Active", Description = "The program has been approved and is actively managed, with projects in progress.", Order = 2, GroupName = nameof(LifecyclePhase.Active))]
    Active = 2,

    [Display(Name = "Completed", Description = "The program has achieved its goals and is considered finished.", Order = 3, GroupName = nameof(LifecyclePhase.Done))]
    Completed = 3,

    [Display(Name = "Cancelled", Description = "The program was terminated before achieving its goals.", Order = 4, GroupName = nameof(LifecyclePhase.Done))]
    Cancelled = 4
}
