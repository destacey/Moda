using Moda.Common.Domain.Enums;
using Moda.Work.Application.BacklogCategories.Dtos;

namespace Moda.Work.Application.BacklogCategories.Queries;

public sealed record GetBacklogCategoriesQuery : IQuery<IReadOnlyList<BacklogCategoryDto>> { }

internal sealed class GetBacklogCategoriesQueryHandler : IQueryHandler<GetBacklogCategoriesQuery, IReadOnlyList<BacklogCategoryDto>>
{
    public Task<IReadOnlyList<BacklogCategoryDto>> Handle(GetBacklogCategoriesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<BacklogCategoryDto> values = Enum.GetValues<BacklogCategory>().Select(c => new BacklogCategoryDto
        {
            Id = (int)c,
            Name = c.GetDisplayName(),
            Description = c.GetDisplayDescription(),
            Order = c.GetDisplayOrder()
        }).ToList();

        return Task.FromResult(values);
    }
}
