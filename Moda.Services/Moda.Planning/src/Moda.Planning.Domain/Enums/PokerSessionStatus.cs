using System.ComponentModel.DataAnnotations;

namespace Moda.Planning.Domain.Enums;

// max length of 32 characters
public enum PokerSessionStatus
{
    [Display(Name = "Created", Description = "The poker session has been created but not yet started.", Order = 1)]
    Created = 1,

    [Display(Name = "Active", Description = "The poker session is active and accepting votes.", Order = 2)]
    Active = 2,

    [Display(Name = "Completed", Description = "The poker session has been completed.", Order = 3)]
    Completed = 3,
}
