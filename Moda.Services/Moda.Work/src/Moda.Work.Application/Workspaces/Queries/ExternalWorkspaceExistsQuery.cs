using Moda.Work.Application.Persistence;

namespace Moda.Work.Application.Workspaces.Queries;
public sealed record ExternalWorkspaceExistsQuery(string ExternalId) : IQuery<bool>;

internal sealed class ExternalWorkspaceExistsQueryHandler(IWorkDbContext workDbContext) : IQueryHandler<ExternalWorkspaceExistsQuery, bool>
{
    private readonly IWorkDbContext _workDbContext = workDbContext;

    public async Task<bool> Handle(ExternalWorkspaceExistsQuery request, CancellationToken cancellationToken)
    {
        return await _workDbContext.Workspaces
            .AnyAsync(w => w.OwnershipInfo.ExternalId == request.ExternalId, cancellationToken);
    }
}