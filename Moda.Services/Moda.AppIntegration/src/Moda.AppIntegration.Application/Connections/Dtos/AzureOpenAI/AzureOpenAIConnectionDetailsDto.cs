using Mapster;
using Wayd.AppIntegration.Domain.Models.AzureOpenAI;

namespace Wayd.AppIntegration.Application.Connections.Dtos.AzureOpenAI;

public sealed record AzureOpenAIConnectionDetailsDto : ConnectionDetailsDto, IMapFrom<AzureOpenAIConnection>
{
    /// <summary>
    /// The configuration for the Azure OpenAI connection.
    /// </summary>
    public required AzureOpenAIConnectionConfigurationDto Configuration { get; set; }

    public override void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<AzureOpenAIConnection, AzureOpenAIConnectionDetailsDto>()
            .Inherits<Connection, ConnectionDetailsDto>();
    }
}
