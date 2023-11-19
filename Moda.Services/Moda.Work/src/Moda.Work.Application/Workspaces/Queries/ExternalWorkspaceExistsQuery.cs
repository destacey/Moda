using Microsoft.EntityFrameworkCore;

namespace Moda.Work.Application.Workspaces.Queries;
public sealed record ExternalWorkspaceExistsQuery(Guid ExternalId): IQuery<bool>;

internal sealed class ExternalWorkspaceExistsQueryHandler : IQueryHandler<ExternalWorkspaceExistsQuery, bool>
{
    private readonly IWorkDbContext _workDbContext;

    public ExternalWorkspaceExistsQueryHandler(IWorkDbContext workDbContext)
    {
        _workDbContext = workDbContext;
    }

    public async Task<bool> Handle(ExternalWorkspaceExistsQuery request, CancellationToken cancellationToken)
    {
        return await _workDbContext.Workspaces
            .AnyAsync(w => w.ExternalId == request.ExternalId, cancellationToken);
    }
}