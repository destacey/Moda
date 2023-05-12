using Moda.Common.Extensions;
using Moda.Planning.Application.Risks.Dtos;
using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.Risks.Queries;

public sealed record GetRiskGradesQuery : IQuery<IReadOnlyList<RiskGradeDto>> { }

internal sealed class GetRiskGradesQueryHandler : IQueryHandler<GetRiskGradesQuery, IReadOnlyList<RiskGradeDto>>
{
    public Task<IReadOnlyList<RiskGradeDto>> Handle(GetRiskGradesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<RiskGradeDto> values = Enum.GetValues<RiskCategory>().Select(c => new RiskGradeDto
        {
            Id = (int)c,
            Name = c.GetDisplayName(),
            Description = c.GetDisplayDescription(),
            Order = c.GetDisplayOrder()
        }).ToList();

        return Task.FromResult(values);
    }
}
