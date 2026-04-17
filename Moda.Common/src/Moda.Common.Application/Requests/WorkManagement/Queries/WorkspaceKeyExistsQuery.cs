using Wayd.Common.Models;

namespace Wayd.Common.Application.Requests.WorkManagement.Queries;

public sealed record WorkspaceKeyExistsQuery : IQuery<bool>
{
    public WorkspaceKeyExistsQuery(string workspaceKey)
    {
        WorkspaceKey = new WorkspaceKey(workspaceKey);
    }

    public WorkspaceKey WorkspaceKey { get; set; }
}