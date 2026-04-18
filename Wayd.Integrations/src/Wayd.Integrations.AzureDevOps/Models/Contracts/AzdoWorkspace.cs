using Wayd.Common.Application.Interfaces.ExternalWork;

namespace Wayd.Integrations.AzureDevOps.Models.Contracts;

public record AzdoWorkspace : IExternalWorkspace
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }
}
