using System.ComponentModel.DataAnnotations;

namespace Moda.StrategicManagement.Domain.Enums;
public enum VisionState
{
    [Display(Name = "Proposed", Description = "The vision is under consideration but not yet active.", Order = 1)]
    Proposed,

    [Display(Name = "Active", Description = "The vision is currently guiding the organization.", Order = 2)]
    Active,

    [Display(Name = "Archived", Description = "The vision is no longer active but retained for historical reference.", Order = 3)]
    Archived
}
