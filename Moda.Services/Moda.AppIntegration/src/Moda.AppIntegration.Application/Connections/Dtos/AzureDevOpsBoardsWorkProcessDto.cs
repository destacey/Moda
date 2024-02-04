using Mapster;

namespace Moda.AppIntegration.Application.Connections.Dtos;
public sealed record AzureDevOpsBoardsWorkProcessDto : IMapFrom<AzureDevOpsBoardsWorkProcess>
{
    public Guid? Id { get; set; }
    public Guid ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
