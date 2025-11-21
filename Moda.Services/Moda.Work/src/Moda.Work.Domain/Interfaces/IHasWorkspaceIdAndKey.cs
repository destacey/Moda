using Moda.Common.Models;

namespace Moda.Work.Domain.Interfaces;
public interface IHasWorkspaceIdAndKey
{
    Guid Id { get; }
    WorkspaceKey Key { get; }
}
