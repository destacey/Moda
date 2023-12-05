using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Integrations.AzureDevOps.Models;
public sealed record AzdoWorkProcessDetails : AzdoWorkProcess, IExternalWorkProcessDetails
{
    public IEnumerable<IExternalBacklogLevel> Behaviors { get; set; } = new List<IExternalBacklogLevel>();
    public IEnumerable<IExternalWorkType> WorkTypes { get; set; } = new List<IExternalWorkType>();
}
