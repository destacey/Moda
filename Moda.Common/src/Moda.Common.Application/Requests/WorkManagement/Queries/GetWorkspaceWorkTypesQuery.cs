using Ardalis.GuardClauses;
using Moda.Common.Application.Requests.WorkManagement.Interfaces;
using Moda.Common.Models;

namespace Moda.Common.Application.Requests.WorkManagement.Queries;
public sealed record GetWorkspaceWorkTypesQuery : IQuery<Result<IReadOnlyList<IWorkTypeDto>>>
{
    public GetWorkspaceWorkTypesQuery(Guid workspaceId)
    {
        Id = Guard.Against.NullOrEmpty(workspaceId);
    }

    public GetWorkspaceWorkTypesQuery(WorkspaceKey workspaceKey)
    {
        Key = Guard.Against.Default(workspaceKey);
    }

    public Guid? Id { get; }
    public WorkspaceKey? Key { get; }
}
