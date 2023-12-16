using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Integrations.AzureDevOps.Models;
public record AzdoWorkType : IExternalWorkType
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string BacklogLevelId { get; set; }
    public bool IsDisabled { get; set; }
}
