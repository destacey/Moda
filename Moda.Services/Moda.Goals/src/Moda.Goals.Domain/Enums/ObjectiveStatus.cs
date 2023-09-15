using System.ComponentModel.DataAnnotations;

namespace Moda.Goals.Domain.Enums;
public enum ObjectiveStatus
{
    [Display(Name = "Not Started", Description = "The objective has not been started.", Order = 1)]
    NotStarted = 1,

    [Display(Name = "In Progress", Description = "The objective is in progress.", Order = 2)]
    InProgress = 2,

    [Display(Name = "Closed", Description = "The objective has been closed.", Order = 3)]
    Closed = 3,

    [Display(Name = "Canceled", Description = "The objective was canceled.", Order = 4)]
    Canceled = 4,
}
