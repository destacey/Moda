using Wayd.Common.Application.Interfaces.ExternalWork;

namespace Wayd.Integrations.AzureDevOps.Models.Contracts;

public sealed record AzdoWorkStatus : IExternalWorkStatus
{
    public required string Name { get; set; }
}
