using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums.StrategicManagement;

// max length of 32 characters

public enum StrategicThemeState
{
    [Display(Name = "Proposed", Description = "The theme is being considered but not yet adopted.", Order = 1, GroupName = nameof(LifecyclePhase.NotStarted))]
    Proposed = 1,

    [Display(Name = "Active", Description = "The theme is currently guiding related initiatives.", Order = 2, GroupName = nameof(LifecyclePhase.Active))]
    Active = 2,

    [Display(Name = "Archived", Description = "The theme is no longer active but retained for historical purposes.", Order = 3, GroupName = nameof(LifecyclePhase.Done))]
    Archived = 3
}
