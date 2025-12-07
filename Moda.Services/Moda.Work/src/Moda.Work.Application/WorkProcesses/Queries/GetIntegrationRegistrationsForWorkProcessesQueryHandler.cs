using Moda.Common.Application.Requests.WorkManagement.Queries;
using Moda.Common.Domain.Models;
using Moda.Work.Application.Persistence;

namespace Moda.Work.Application.WorkProcesses.Queries;

internal sealed class GetIntegrationRegistrationsForWorkProcessesQueryHandler(IWorkDbContext workDbContext) : IQueryHandler<GetIntegrationRegistrationsForWorkProcessesQuery, List<IntegrationRegistration<Guid, Guid>>>
{
    private readonly IWorkDbContext _workDbContext = workDbContext;

    public async Task<List<IntegrationRegistration<Guid, Guid>>> Handle(GetIntegrationRegistrationsForWorkProcessesQuery request, CancellationToken cancellationToken)
    {
        var query = _workDbContext.WorkProcesses
            .Where(w => w.ExternalId.HasValue);

        if (request.ExternalId.HasValue)
            query = query.Where(w => w.ExternalId == request.ExternalId);

        return await query
            .Select(w => new IntegrationRegistration<Guid, Guid>(w.ExternalId!.Value, IntegrationState<Guid>.Create(w.Id, w.IsActive)))
            .ToListAsync(cancellationToken);
    }
}
