using Mapster;
using Moda.Common.Application.Dtos;

namespace Moda.AppIntegration.Application.Connections.Dtos;
public sealed record ConnectionListDto : IMapFrom<Connection>
{
    /// <summary>
    /// The unique identifier for the connection.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the connection.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The unique identifier for the system that this connection connects to. 
    /// </summary>
    public string? SystemId { get; set; }

    /// <summary>
    /// The type of connector for the connection.  This value cannot be changed once set.
    /// </summary>
    public required SimpleNavigationDto Connector { get; set; }

    /// <summary>
    /// Indicates whether the connection is active or not.  Inactive connections are not included in the synchronization process.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// A flag indicating whether the connection configuration is valid.
    /// </summary>
    public bool IsValidConfiguration { get; set; }

    /// <summary>
    /// The indicator for whether the connection is enabled for synchronization.
    /// </summary>
    public bool IsSyncEnabled { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<Connection, ConnectionListDto>()
            .Map(dest => dest.Connector, src => SimpleNavigationDto.FromEnum(src.Connector));
    }
}
