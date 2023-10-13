using Microsoft.TeamFoundation.Core.WebApi;
using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Integrations.AzureDevOps.Models;
public sealed record AzdoWorkProcess : IExternalWorkProcess
{
    public AzdoWorkProcess(Process process)
    {
        Id = process.Id;
        Name = process.Name;
        Description = process.Description;            
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }
}
