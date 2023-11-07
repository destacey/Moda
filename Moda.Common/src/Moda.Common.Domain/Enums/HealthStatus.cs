using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums;
public enum HealthStatus
{
    [Display(Name = "Healthy", Description = "The object is healthy.", Order = 1)]
    Healthy = 1,

    [Display(Name = "At Risk", Description = "The object is at risk of being unhealthy.", Order = 2)]
    AtRisk = 2,

    [Display(Name = "Unhealthy", Description = "The object is owned by Moda, but not changable by a user.", Order = 3)]
    Unhealthy = 3,
}
