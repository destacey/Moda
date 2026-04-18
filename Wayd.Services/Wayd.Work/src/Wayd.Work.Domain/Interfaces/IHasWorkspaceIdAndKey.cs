using Wayd.Common.Models;

namespace Wayd.Work.Domain.Interfaces;

public interface IHasWorkspaceIdAndKey
{
    Guid Id { get; }
    WorkspaceKey Key { get; }
}
