using Wayd.Common.Domain.Models;

namespace Wayd.Common.Application.Requests.WorkManagement.Queries;

public sealed record GetIntegrationRegistrationsForWorkProcessesQuery(Guid? ExternalId = null)
    : IQuery<List<IntegrationRegistration<Guid, Guid>>>;
