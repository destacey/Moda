using Moda.Work.Domain.Models;

namespace Moda.Work.Domain.Interfaces;
public interface HasWorkspace
{
    Guid WorkspaceId { get; }
    Workspace Workspace { get; }
}
