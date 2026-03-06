using Mapster;
using Moda.AppIntegration.Domain.Models.AzureOpenAI;

namespace Moda.AppIntegration.Application.Connections.Dtos.AzureOpenAI;

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
