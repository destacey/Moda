using Wayd.Common.Application.Interfaces.ExternalWork;

namespace Wayd.Integrations.AzureDevOps.Models.Contracts;

public sealed record AzdoWorkspaceConfiguration : AzdoWorkspace, IExternalWorkspaceConfiguration
{
    public Guid WorkProcessId { get; set; }
}
