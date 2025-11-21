using Moda.Work.Domain.Models;

namespace Moda.Work.Domain.Interfaces;
public interface IHasWorkspace
{
    Guid WorkspaceId { get; }
    Workspace Workspace { get; }
}
