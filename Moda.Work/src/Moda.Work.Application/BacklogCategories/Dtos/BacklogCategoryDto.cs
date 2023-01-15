namespace Moda.Work.Application.BacklogCategories.Dtos;

public sealed record BacklogCategoryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
}
