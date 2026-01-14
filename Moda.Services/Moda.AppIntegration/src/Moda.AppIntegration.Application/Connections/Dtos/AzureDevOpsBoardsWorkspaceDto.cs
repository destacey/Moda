using Mapster;
using Moda.Common.Application.Dtos;

namespace Moda.AppIntegration.Application.Connections.Dtos;
public sealed record AzureDevOpsBoardsWorkspaceDto : IMapFrom<AzureDevOpsBoardsWorkspace>
{
    public Guid ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid WorkProcessId { get; set; }
    public IntegrationStateDto? IntegrationState { get; set; }
}
