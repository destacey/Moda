namespace Moda.Common.Application.Interfaces.ExternalWork;
public interface IExternalWorkProcessConfiguration
{
    Guid Id { get; }
    string Name { get; }
    string? Description { get; }
    bool IsEnabled { get; }
    List<Guid> WorkspaceIds { get; }
    IList<IExternalBacklogLevel> BacklogLevels { get; }
    IList<IExternalWorkType> WorkTypes { get; }
    IList<IExternalWorkStatus> WorkStatuses { get; }
}