using Moda.Common.Application.Requests.WorkManagement.Queries;
using Moda.Work.Application.Persistence;

namespace Moda.Work.Application.Workspaces.Queries;

internal sealed class ExternalWorkspaceExistsQueryHandler(IWorkDbContext workDbContext) : IQueryHandler<ExternalWorkspaceExistsQuery, bool>
{
    private readonly IWorkDbContext _workDbContext = workDbContext;

    public async Task<bool> Handle(ExternalWorkspaceExistsQuery request, CancellationToken cancellationToken)
    {
        return await _workDbContext.Workspaces
            .AnyAsync(w => w.OwnershipInfo.ExternalId == request.ExternalId, cancellationToken);
    }
}