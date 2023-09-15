using Microsoft.TeamFoundation.Core.WebApi;
using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Integrations.AzureDevOps.Models;
public class AzdoWorkspace : IExternalWorkspace
{
    public AzdoWorkspace(TeamProject teamProject)
    {
        Id = teamProject.Id;
        Name = teamProject.Name;
        Description = teamProject.Description;
    }

    public AzdoWorkspace(TeamProjectReference teamProject)
    {
        Id = teamProject.Id;
        Name = teamProject.Name;
        Description = teamProject.Description;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

}
