using Moda.Work.Application.Persistence;

namespace Moda.Work.Application.WorkProcesses.Queries;
public sealed record ExternalWorkProcessExistsQuery(Guid ExternalId) : IQuery<bool>;

internal sealed class ExternalWorkProcessExistsQueryHandler : IQueryHandler<ExternalWorkProcessExistsQuery, bool>
{
    private readonly IWorkDbContext _workDbContext;

    public ExternalWorkProcessExistsQueryHandler(IWorkDbContext workDbContext)
    {
        _workDbContext = workDbContext;
    }

    public async Task<bool> Handle(ExternalWorkProcessExistsQuery request, CancellationToken cancellationToken)
    {
        return await _workDbContext.WorkProcesses
            .AnyAsync(w => w.ExternalId == request.ExternalId, cancellationToken);
    }
}