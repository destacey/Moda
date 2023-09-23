﻿using Mapster;

namespace Moda.AppIntegration.Application.Connections.Dtos;
public sealed record AzureDevOpsBoardsWorkspaceDto : IMapFrom<AzureDevOpsBoardsWorkspace>
{
    public Guid ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool Sync { get; set; }

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AzureDevOpsBoardsWorkspace, AzureDevOpsBoardsWorkspaceDto>();
    }
}