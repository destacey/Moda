using Mapster;

namespace Moda.AppIntegration.Application.Connections.Dtos;
public sealed record ConnectionListDto : IMapFrom<Connection>
{
    /// <summary>Gets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the name of the connection.</summary>
    /// <value>The name of the connection.</value>
    public required string Name { get; set; }

    /// <summary>Gets the type of connector.  This value cannot change.</summary>
    /// <value>The type of connector.</value>
    public required string Connector { get; set; }

    /// <summary>
    /// Indicates whether the connection is active or not.  Inactive connections are not included in the synchronization process.
    /// </summary>
    /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance has a valid configuration.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is valid configuration; otherwise, <c>false</c>.
    /// </value>
    public bool IsValidConfiguration { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<Connection, ConnectionListDto>()
            .Map(dest => dest.Connector, src => src.Connector.GetDisplayName());
    }
}
