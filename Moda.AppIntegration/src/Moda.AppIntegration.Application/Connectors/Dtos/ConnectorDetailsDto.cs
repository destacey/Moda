using Mapster;

namespace Moda.AppIntegration.Application.Connectors.Dtos;
public sealed record ConnectorDetailsDto : IRegister
{
    /// <summary>Gets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the name of the connector.</summary>
    /// <value>The name of the connector.</value>
    public required string Name { get; set; }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The connector description.</value>
    public string? Description { get; set; }

    /// <summary>Gets the type of connector.  This value cannot change.</summary>
    /// <value>The type of connector.</value>
    public required string Type { get; set; }

    /// <summary>
    /// Indicates whether the connector is active or not.  Inactive connectors are not included in the synchronization process.
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

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Connector, ConnectorDetailsDto>()
            .Map(dest => dest.Type, src => src.Type.GetDisplayName());
    }
}
