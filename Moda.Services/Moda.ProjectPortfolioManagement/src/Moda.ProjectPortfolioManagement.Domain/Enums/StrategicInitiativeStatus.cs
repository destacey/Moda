﻿using System.ComponentModel.DataAnnotations;

namespace Moda.ProjectPortfolioManagement.Domain.Enums;

// max length of 32 characters

/// <summary>
/// Represents the status of a strategic initiative in its lifecycle.
/// </summary>
public enum StrategicInitiativeStatus
{
    [Display(Name = "Proposed", Description = "The strategic initiative is in the initial planning or conceptualization stage. It has not yet been approved or activated.", Order = 1)]
    Proposed = 1,

    [Display(Name = "Approved", Description = "The strategic initiative has been reviewed and approved, but has not started yet.", Order = 2)]
    Approved = 2,

    [Display(Name = "Active", Description = "The strategic initiative has been approved and is actively managed, with projects in progress.", Order = 3)]
    Active = 3,

    [Display(Name = "On Hold", Description = "Work on the strategic initiative has been temporarily paused due to resource constraints, shifting priorities, or unforeseen challenges.", Order = 4)]
    OnHold = 4,

    [Display(Name = "Completed", Description = "The strategic initiative has achieved its goals and is considered finished.", Order = 5)]
    Completed = 5,

    [Display(Name = "Cancelled", Description = "The strategic initiative was terminated before achieving its goals.", Order = 6)]
    Cancelled = 6
}
