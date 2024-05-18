using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Integrations.AzureDevOps.Models.Contracts;
public sealed record AzdoWorkStatus : IExternalWorkStatus
{
    public required string Name { get; set; }
}
