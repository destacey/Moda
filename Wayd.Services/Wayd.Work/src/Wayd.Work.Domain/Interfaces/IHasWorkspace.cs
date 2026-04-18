using Wayd.Work.Domain.Models;

namespace Wayd.Work.Domain.Interfaces;

public interface IHasWorkspace
{
    Guid WorkspaceId { get; }
    Workspace Workspace { get; }
}
