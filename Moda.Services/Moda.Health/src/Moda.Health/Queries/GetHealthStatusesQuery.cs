using Wayd.Common.Application.Interfaces;
using Wayd.Common.Domain.Enums;
using Wayd.Common.Extensions;
using Wayd.Health.Dtos;

namespace Wayd.Health.Queries;

public sealed record GetHealthStatusesQuery : IQuery<IReadOnlyList<HealthStatusDto>> { }

internal sealed class GetHealthStatusesQueryHandler : IQueryHandler<GetHealthStatusesQuery, IReadOnlyList<HealthStatusDto>>
{
    public Task<IReadOnlyList<HealthStatusDto>> Handle(GetHealthStatusesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<HealthStatusDto> values = Enum.GetValues<HealthStatus>().Select(c => new HealthStatusDto
        {
            Id = (int)c,
            Name = c.GetDisplayName(),
            Description = c.GetDisplayDescription(),
            Order = c.GetDisplayOrder()
        }).ToList();

        return Task.FromResult(values);
    }
}
