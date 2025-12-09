using Moda.Common.Domain.Models;

namespace Moda.Common.Application.Requests.WorkManagement.Queries;

public sealed record GetIntegrationRegistrationsForWorkProcessesQuery(Guid? ExternalId = null) 
    : IQuery<List<IntegrationRegistration<Guid, Guid>>>;
