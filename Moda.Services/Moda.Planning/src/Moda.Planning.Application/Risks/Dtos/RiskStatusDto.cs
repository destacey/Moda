namespace Moda.Planning.Application.Risks.Dtos;
public sealed record RiskStatusDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
}
