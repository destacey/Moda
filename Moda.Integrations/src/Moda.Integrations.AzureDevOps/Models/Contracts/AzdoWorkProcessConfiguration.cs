using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Integrations.AzureDevOps.Models.Contracts;
public sealed record AzdoWorkProcessConfiguration : AzdoWorkProcess, IExternalWorkProcessConfiguration
{
    public IList<IExternalWorkTypeLevel> WorkTypeLevels { get; set; } = [];
    public IList<IExternalWorkTypeWorkflow> WorkTypes { get; set; } = [];
    public IList<IExternalWorkStatus> WorkStatuses { get; set; } = [];
}
