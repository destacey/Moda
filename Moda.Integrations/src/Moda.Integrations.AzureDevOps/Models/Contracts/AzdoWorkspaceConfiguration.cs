using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Integrations.AzureDevOps.Models.Contracts;
public record AzdoWorkspaceConfiguration : AzdoWorkspace, IExternalWorkspaceConfiguration
{
    public Guid WorkProcessId { get; set; }
}
