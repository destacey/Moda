using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Integrations.AzureDevOps.Models;
public sealed record AzdoBacklogLevel : IExternalBacklogLevel
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Rank { get; set; }
}
