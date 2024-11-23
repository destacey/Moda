using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums.Work;

public enum WorkItemLinkType
{
    // Current Db length is set to 32 characters
    [Display(Name = "Hierarchy", Description = "A link that defines a hierarchy between work items.", Order = 1)]
    Hierarchy = 1,

    [Display(Name = "Dependency", Description = "A link that defines a predecessor/successor dependency between work items.", Order = 2)]
    Dependency = 2,

    // TODO: add a link type for work items to other objects like objectives, risks, etc.
}
