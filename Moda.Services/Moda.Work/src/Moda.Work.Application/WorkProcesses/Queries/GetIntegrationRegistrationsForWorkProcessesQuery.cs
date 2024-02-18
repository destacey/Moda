using Moda.Common.Domain.Models;

namespace Moda.Work.Application.WorkProcesses.Queries;
public sealed record GetIntegrationRegistrationsForWorkProcessesQuery : IQuery<List<IntegrationRegistration<Guid, Guid>>>;

internal sealed class GetIntegrationRegistrationsForWorkProcessesQueryHandler(IWorkDbContext workDbContext) : IQueryHandler<GetIntegrationRegistrationsForWorkProcessesQuery, List<IntegrationRegistration<Guid, Guid>>>
{
    private readonly IWorkDbContext _workDbContext = workDbContext;

    public async Task<List<IntegrationRegistration<Guid, Guid>>> Handle(GetIntegrationRegistrationsForWorkProcessesQuery request, CancellationToken cancellationToken)
    {
        return await _workDbContext.WorkProcesses
            .Where(w => w.ExternalId.HasValue)
            .Select(w => new IntegrationRegistration<Guid, Guid>(w.ExternalId!.Value, IntegrationState<Guid>.Create(w.Id, w.IsActive)))
            .ToListAsync(cancellationToken);
    }
}
