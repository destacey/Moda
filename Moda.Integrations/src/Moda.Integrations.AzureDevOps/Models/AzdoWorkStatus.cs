using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Integrations.AzureDevOps.Models;
public record AzdoWorkStatus : IExternalWorkStatus
{
    public required string Name { get; set; }
}
