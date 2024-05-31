using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Domain.Enums.Work;

namespace Moda.Integrations.AzureDevOps.Models.Contracts;
public sealed record AzdoWorkTypeLevel : IExternalWorkTypeLevel
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public WorkTypeTier Tier { get; set; }
    public int Order { get; set; }
}
