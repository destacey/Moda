namespace Moda.Common.Domain.Models;

/// <summary>
/// Represents the registration of an external id with the internal object's id and state.
/// </summary>
/// <typeparam name="TE">Type of the ExternalId</typeparam>
/// <typeparam name="TI">Type of the InternalId</typeparam>
/// <param name="ExternalId"></param>
/// <param name="IntegrationState"></param>
public sealed record IntegrationRegistration<TE, TI>(TE ExternalId, IntegrationState<TI> IntegrationState);
