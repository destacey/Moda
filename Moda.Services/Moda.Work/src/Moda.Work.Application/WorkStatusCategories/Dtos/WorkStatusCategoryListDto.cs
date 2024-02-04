namespace Moda.Work.Application.WorkStatusCategories.Dtos;

public sealed record WorkStatusCategoryListDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
}
