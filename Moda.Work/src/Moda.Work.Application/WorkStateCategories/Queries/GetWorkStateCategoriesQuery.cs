namespace Moda.Work.Application.WorkStateCategories.Queries;

public sealed record GetWorkStateCategoriesQuery : IQuery<IReadOnlyList<WorkStateCategoryListDto>> { }

internal sealed class GetWorkStateCategoriesQueryHandler : IQueryHandler<GetWorkStateCategoriesQuery, IReadOnlyList<WorkStateCategoryListDto>>
{
    public Task<IReadOnlyList<WorkStateCategoryListDto>> Handle(GetWorkStateCategoriesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<WorkStateCategoryListDto> values = Enum.GetValues<WorkStateCategory>().Select(c => new WorkStateCategoryListDto
        {
            Id = (int)c,
            Name = c.GetDisplayName(),
            Description = c.GetDisplayDescription()
        }).ToList();

        return Task.FromResult(values);
    }
}
