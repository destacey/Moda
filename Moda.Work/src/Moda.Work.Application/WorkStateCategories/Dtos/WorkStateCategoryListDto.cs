namespace Moda.Work.Application.WorkStateCategories.Dtos;

public sealed record WorkStateCategoryListDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
