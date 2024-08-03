namespace Moda.Common.Application.Interfaces.ExternalWork;
public interface IExternalTeam
{
    Guid Id { get; }
    string Name { get; }
    Guid WorkspaceId { get; }
}
