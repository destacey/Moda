using Moda.Common.Application.Requests.WorkManagement.Queries;
using Moda.Work.Application.Persistence;

namespace Moda.Work.Application.Workspaces.Queries;

internal sealed class WorkspaceKeyExistsQueryHandler(IWorkDbContext workDbContext) : IQueryHandler<WorkspaceKeyExistsQuery, bool>
{
    private readonly IWorkDbContext _workDbContext = workDbContext;

    public async Task<bool> Handle(WorkspaceKeyExistsQuery request, CancellationToken cancellationToken)
    {
        return await _workDbContext.Workspaces
            .AnyAsync(w => w.Key == request.WorkspaceKey, cancellationToken);
    }
}