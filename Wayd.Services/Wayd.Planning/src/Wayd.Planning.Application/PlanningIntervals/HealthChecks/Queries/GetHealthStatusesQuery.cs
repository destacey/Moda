using Wayd.Common.Domain.Enums;
using Wayd.Common.Extensions;
using Wayd.Planning.Application.PlanningIntervals.HealthChecks.Dtos;

namespace Wayd.Planning.Application.PlanningIntervals.HealthChecks.Queries;

public sealed record GetHealthStatusesQuery : IQuery<IReadOnlyList<HealthStatusDto>>;

internal sealed class GetHealthStatusesQueryHandler : IQueryHandler<GetHealthStatusesQuery, IReadOnlyList<HealthStatusDto>>
{
    public Task<IReadOnlyList<HealthStatusDto>> Handle(GetHealthStatusesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<HealthStatusDto> values = [.. Enum.GetValues<HealthStatus>().Select(c => new HealthStatusDto
        {
            Id = (int)c,
            Name = c.GetDisplayName(),
            Description = c.GetDisplayDescription(),
            Order = c.GetDisplayOrder()
        })];

        return Task.FromResult(values);
    }
}
