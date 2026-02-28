using System.ComponentModel.DataAnnotations;

namespace Moda.Planning.Domain.Enums;

// max length of 32 characters
public enum PokerRoundStatus
{
    [Display(Name = "Pending", Description = "The round has not started voting.", Order = 1)]
    Pending = 1,

    [Display(Name = "Voting", Description = "The round is currently accepting votes.", Order = 2)]
    Voting = 2,

    [Display(Name = "Revealed", Description = "The votes have been revealed.", Order = 3)]
    Revealed = 3,

    [Display(Name = "Accepted", Description = "The consensus estimate has been accepted.", Order = 4)]
    Accepted = 4,
}
