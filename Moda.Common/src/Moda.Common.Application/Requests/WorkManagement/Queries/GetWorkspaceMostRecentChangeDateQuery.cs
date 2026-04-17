using Ardalis.GuardClauses;
using Wayd.Common.Models;

namespace Wayd.Common.Application.Requests.WorkManagement.Queries;

public sealed record GetWorkspaceMostRecentChangeDateQuery : IQuery<Result<Instant?>>
{
    public GetWorkspaceMostRecentChangeDateQuery(Guid workspaceId)
    {
        Id = Guard.Against.NullOrEmpty(workspaceId);
    }

    public GetWorkspaceMostRecentChangeDateQuery(WorkspaceKey workspaceKey)
    {
        Key = Guard.Against.Default(workspaceKey);
    }

    public Guid? Id { get; }
    public WorkspaceKey? Key { get; }
}
