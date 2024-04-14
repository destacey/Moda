using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Application.Enums;
public enum SyncType
{
    [Display(Name = "Full", Description = "Full sync of all data.", Order = 1)]
    Full = 0,

    [Display(Name = "Differential", Description = "Differential sync of data.", Order = 2)]
    Differential = 1
}
