using Moda.Common.Models;

namespace Moda.Work.Domain.Interfaces;
public interface HasWorkspaceIdAndKey
{
    Guid Id { get; }
    WorkspaceKey Key { get; }
}
