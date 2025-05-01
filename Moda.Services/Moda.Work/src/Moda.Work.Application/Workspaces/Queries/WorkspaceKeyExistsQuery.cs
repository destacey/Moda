using Moda.Common.Models;
using Moda.Work.Application.Persistence;

namespace Moda.Work.Application.Workspaces.Queries;
public sealed record WorkspaceKeyExistsQuery : IQuery<bool>
{
    public WorkspaceKeyExistsQuery(string workspaceKey)
    {
        WorkspaceKey = new WorkspaceKey(workspaceKey);
    }

    public WorkspaceKey WorkspaceKey { get; set; }
}

internal sealed class WorkspaceKeyExistsQueryHandler : IQueryHandler<WorkspaceKeyExistsQuery, bool>
{
    private readonly IWorkDbContext _workDbContext;

    public WorkspaceKeyExistsQueryHandler(IWorkDbContext workDbContext)
    {
        _workDbContext = workDbContext;
    }

    public async Task<bool> Handle(WorkspaceKeyExistsQuery request, CancellationToken cancellationToken)
    {
        return await _workDbContext.Workspaces
            .AnyAsync(w => w.Key == request.WorkspaceKey, cancellationToken);
    }
}