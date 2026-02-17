using Mapster;

namespace Moda.AppIntegration.Application.Connections.Dtos.AzureDevOps;

public sealed record AzureDevOpsConnectionListDto : ConnectionListDto, IMapFrom<AzureDevOpsBoardsConnection>
{
    public override void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<AzureDevOpsBoardsConnection, AzureDevOpsConnectionListDto>()
            .Inherits<Connection, ConnectionListDto>();
    }
}
