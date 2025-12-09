using Moda.Common.Application.Requests.WorkManagement.Queries;
using Moda.Work.Application.Persistence;

namespace Moda.Work.Application.WorkProcesses.Queries;

internal sealed class ExternalWorkProcessExistsQueryHandler(IWorkDbContext workDbContext) : IQueryHandler<ExternalWorkProcessExistsQuery, bool>
{
    private readonly IWorkDbContext _workDbContext = workDbContext;

    public async Task<bool> Handle(ExternalWorkProcessExistsQuery request, CancellationToken cancellationToken)
    {
        return await _workDbContext.WorkProcesses
            .AnyAsync(w => w.ExternalId == request.ExternalId, cancellationToken);
    }
}