namespace Moda.Common.Application.Interfaces.ExternalWork;
public interface IExternalWorkProcessConfiguration : IExternalWorkProcess
{
    IList<IExternalBacklogLevel> BacklogLevels { get; }
    IList<IExternalWorkType> WorkTypes { get; }
    IList<IExternalWorkStatus> WorkStatuses { get; }
}