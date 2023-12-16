using Microsoft.EntityFrameworkCore;

namespace Moda.Work.Application.Workspaces.Queries;
public sealed record WorkspaceKeyExistsQuery(WorkspaceKey WorkspaceKey): IQuery<bool>;

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