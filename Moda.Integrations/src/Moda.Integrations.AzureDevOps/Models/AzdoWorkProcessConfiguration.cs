using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Integrations.AzureDevOps.Models;
public sealed record AzdoWorkProcessConfiguration : AzdoWorkProcess, IExternalWorkProcessConfiguration
{
    public IEnumerable<IExternalBacklogLevel> BacklogLevels { get; set; } = new List<IExternalBacklogLevel>();
    public IEnumerable<IExternalWorkType> WorkTypes { get; set; } = new List<IExternalWorkType>();
}
