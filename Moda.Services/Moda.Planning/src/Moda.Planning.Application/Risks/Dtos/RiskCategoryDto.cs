namespace Moda.Planning.Application.Risks.Dtos;
public sealed record RiskCategoryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
}
