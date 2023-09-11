using Moda.Common.Extensions;
using Moda.Planning.Application.Risks.Dtos;
using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.Risks.Queries;

public sealed record GetRiskStatusesQuery : IQuery<IReadOnlyList<RiskStatusDto>> { }

internal sealed class GetRiskStatusesQueryHandler : IQueryHandler<GetRiskStatusesQuery, IReadOnlyList<RiskStatusDto>>
{
    public Task<IReadOnlyList<RiskStatusDto>> Handle(GetRiskStatusesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<RiskStatusDto> values = Enum.GetValues<RiskStatus>().Select(c => new RiskStatusDto
        {
            Id = (int)c,
            Name = c.GetDisplayName(),
            Description = c.GetDisplayDescription(),
            Order = c.GetDisplayOrder()
        }).ToList();

        return Task.FromResult(values);
    }
}
