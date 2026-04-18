using Wayd.Common.Application.Interfaces.ExternalWork;
using Wayd.Common.Domain.Enums.Work;

namespace Wayd.Integrations.AzureDevOps.Models.Contracts;

public sealed record AzdoWorkTypeLevel : IExternalWorkTypeLevel
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public WorkTypeTier Tier { get; set; }
    public int Order { get; set; }
}
