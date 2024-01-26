using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Integrations.AzureDevOps.Models;
public sealed record AzdoWorkProcessConfiguration : AzdoWorkProcess, IExternalWorkProcessConfiguration
{
    public IReadOnlyList<IExternalBacklogLevel> BacklogLevels { get; set; } = [];
    public IReadOnlyList<IExternalWorkType> WorkTypes { get; set; } = [];
    public IReadOnlyList<IExternalWorkStatus> WorkStatuses { get; set; } = [];
}
