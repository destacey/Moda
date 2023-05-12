using Moda.Common.Extensions;
using Moda.Planning.Application.Risks.Dtos;
using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.Risks.Queries;

public sealed record GetRiskCategoriesQuery : IQuery<IReadOnlyList<RiskCategoryDto>> { }

internal sealed class GetRiskCategoriesQueryHandler : IQueryHandler<GetRiskCategoriesQuery, IReadOnlyList<RiskCategoryDto>>
{
    public Task<IReadOnlyList<RiskCategoryDto>> Handle(GetRiskCategoriesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<RiskCategoryDto> values = Enum.GetValues<RiskCategory>().Select(c => new RiskCategoryDto
        {
            Id = (int)c,
            Name = c.GetDisplayName(),
            Description = c.GetDisplayDescription(),
            Order = c.GetDisplayOrder()
        }).ToList();

        return Task.FromResult(values);
    }
}
