namespace Moda.AppIntegration.Application.Connectors.Dtos;
public sealed class ConnectorListDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
