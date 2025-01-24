using System.ComponentModel.DataAnnotations;

namespace Moda.StrategicManagement.Domain.Enums;

// max length of 32 characters

public enum StrategyStatus
{
    [Display(Name = "Draft", Description = "The strategy is in the planning phase.", Order = 1)]
    Draft = 1,

    [Display(Name = "Active", Description = "The strategy is currently being implemented.", Order = 2)]
    Active = 2,

    [Display(Name = "Completed", Description = "The strategy has been successfully executed.", Order = 3)]
    Completed = 3,

    [Display(Name = "Archived", Description = "The strategy is no longer active but retained for historical purposes.", Order = 4)]
    Archived = 4
}
