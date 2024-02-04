using System.Text.Json.Serialization;
using Moda.Common.Domain.Models;

namespace Moda.AppIntegration.Domain.Models;
public abstract class IntegrationObject<TId>
{
    [JsonInclude]
    public IntegrationState<TId>? IntegrationState { get; protected set; }

    public bool HasIntegration => IntegrationState is not null;

    public bool IntegrationIsActive => IntegrationState?.IsActive ?? false;

    public void UpdateIntegrationState(bool isActive)
    {
        if (IntegrationState is null)
            throw new InvalidOperationException("Integration state is not set.");

        IntegrationState?.SetIsActive(isActive);
    }

    public void AddIntegrationState(IntegrationState<TId> integrationState)
    {
        if (IntegrationState is not null)
            throw new InvalidOperationException("Integration state is already set.");

        IntegrationState = integrationState;
    }
}
