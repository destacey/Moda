using System.ComponentModel.DataAnnotations;

namespace Moda.StrategicManagement.Domain.Enums;

// max length of 32 characters

public enum VisionState
{
    [Display(Name = "Proposed", Description = "The vision is under consideration but not yet active.", Order = 1)]
    Proposed = 1,

    [Display(Name = "Active", Description = "The vision is currently guiding the organization.", Order = 2)]
    Active = 2,

    [Display(Name = "Archived", Description = "The vision is no longer active but retained for historical reference.", Order = 3)]
    Archived = 3
}
