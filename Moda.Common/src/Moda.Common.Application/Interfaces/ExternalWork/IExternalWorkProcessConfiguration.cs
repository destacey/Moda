namespace Moda.Common.Application.Interfaces.ExternalWork;
public interface IExternalWorkProcessConfiguration
{
    Guid Id { get; }
    string Name { get; }
    string? Description { get; }
    bool IsEnabled { get; }
    List<Guid> WorkspaceIds { get; }
    IReadOnlyList<IExternalBacklogLevel> BacklogLevels { get; }
    IReadOnlyList<IExternalWorkType> WorkTypes { get; }
    IReadOnlyList<IExternalWorkStatus> WorkStatuses { get; }
}