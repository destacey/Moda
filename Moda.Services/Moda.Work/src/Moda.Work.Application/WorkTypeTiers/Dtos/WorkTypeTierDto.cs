namespace Moda.Work.Application.WorkTypeTiers.Dtos;

public sealed record WorkTypeTierDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
}
