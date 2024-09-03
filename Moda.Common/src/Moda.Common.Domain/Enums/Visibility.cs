using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums;
public enum Visibility
{
    [Display(Name = "Public", Description = "The object is visible to all users.", Order = 1)]
    Public = 1,

    [Display(Name = "Private", Description = "The object is only visible to specific users.", Order = 2)]
    Private = 2
}
