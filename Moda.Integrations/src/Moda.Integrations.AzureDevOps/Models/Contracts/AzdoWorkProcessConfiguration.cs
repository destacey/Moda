using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Integrations.AzureDevOps.Models.Contracts;
public sealed record AzdoWorkProcessConfiguration : AzdoWorkProcess, IExternalWorkProcessConfiguration
{
    public IList<IExternalBacklogLevel> BacklogLevels { get; set; } = [];
    public IList<IExternalWorkType> WorkTypes { get; set; } = [];
    public IList<IExternalWorkStatus> WorkStatuses { get; set; } = [];
}
