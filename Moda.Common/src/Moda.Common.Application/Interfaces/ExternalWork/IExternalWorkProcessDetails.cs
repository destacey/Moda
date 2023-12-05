namespace Moda.Common.Application.Interfaces.ExternalWork;
public interface IExternalWorkProcessDetails
{
    Guid Id { get; }
    string Name { get; }
    string? Description { get; }
    bool IsEnabled { get; }
    List<Guid> WorkspaceIds { get; }
    IEnumerable<IExternalBacklogLevel> Behaviors { get; }
}