using Wayd.Common.Application.Interfaces.ExternalWork;

namespace Wayd.Integrations.AzureDevOps.Models.Contracts;

public record AzdoWorkProcess : IExternalWorkProcess
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public List<Guid> WorkspaceIds { get; set; } = [];

    public bool IsEnabled { get; set; }
}
