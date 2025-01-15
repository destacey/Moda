using System.ComponentModel.DataAnnotations;

namespace Moda.StrategicManagement.Domain.Enums;

// max length of 32 characters

public enum StrategicThemeState
{
    [Display(Name = "Proposed", Description = "The theme is being considered but not yet adopted.", Order = 1)]
    Proposed,

    [Display(Name = "Active", Description = "The theme is currently guiding related initiatives.", Order = 2)]
    Active,

    [Display(Name = "Archived", Description = "The theme is no longer active but retained for historical purposes.", Order = 3)]
    Archived
}
