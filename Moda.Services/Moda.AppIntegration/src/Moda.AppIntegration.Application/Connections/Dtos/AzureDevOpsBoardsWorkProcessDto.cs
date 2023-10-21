using Mapster;

namespace Moda.AppIntegration.Application.Connections.Dtos;
public sealed record AzureDevOpsBoardsWorkProcessDto : IMapFrom<AzureDevOpsBoardsWorkProcess>
{
    public Guid ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<AzureDevOpsBoardsWorkProcess, AzureDevOpsBoardsWorkProcessDto>();
    }
}
