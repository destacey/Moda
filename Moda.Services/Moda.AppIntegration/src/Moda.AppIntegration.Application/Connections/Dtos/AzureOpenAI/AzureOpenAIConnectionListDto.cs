using Mapster;
using Moda.AppIntegration.Domain.Models.AzureOpenAI;

namespace Moda.AppIntegration.Application.Connections.Dtos.AzureOpenAI;

public sealed record AzureOpenAIConnectionListDto : ConnectionListDto, IMapFrom<AzureOpenAIConnection>
{
    public override void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<AzureOpenAIConnection, AzureOpenAIConnectionListDto>()
            .Inherits<Connection, ConnectionListDto>();
    }
}
