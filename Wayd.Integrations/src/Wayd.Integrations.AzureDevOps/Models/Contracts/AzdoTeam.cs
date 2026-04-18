using Wayd.Common.Application.Interfaces.ExternalWork;

namespace Wayd.Integrations.AzureDevOps.Models.Contracts;

public sealed record AzdoTeam : IExternalTeam
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public Guid WorkspaceId { get; set; }

    public Guid? BoardId { get; set; }
}
