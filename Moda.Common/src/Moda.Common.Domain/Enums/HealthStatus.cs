using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums;
public enum HealthStatus
{
    [Display(Description = "The object is healthy.")]
    Healthy = 0,

    [Display(Description = "The object is at risk of being unhealthy.")]
    AtRisk = 1,

    [Display(Description = "The object is owned by Moda, but not changable by a user.")]
    Unhealthy = 2,
}
