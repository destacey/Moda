using Mapster;
using Wayd.Common.Application.Dtos;

namespace Wayd.AppIntegration.Application.Connections.Dtos.AzureDevOps;

public sealed record AzureDevOpsWorkspaceDto : IMapFrom<AzureDevOpsBoardsWorkspace>
{
    public Guid ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid WorkProcessId { get; set; }
    public IntegrationStateDto? IntegrationState { get; set; }
}
