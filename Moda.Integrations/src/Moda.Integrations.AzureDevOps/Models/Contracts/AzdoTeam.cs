using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Integrations.AzureDevOps.Models.Contracts;
public sealed record AzdoTeam : IExternalTeam
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public Guid WorkspaceId { get; set; }
}
