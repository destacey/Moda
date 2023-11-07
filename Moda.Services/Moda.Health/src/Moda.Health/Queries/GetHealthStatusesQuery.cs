using Moda.Common.Application.Interfaces;
using Moda.Common.Domain.Enums;
using Moda.Common.Extensions;
using Moda.Health.Dtos;

namespace Moda.Health.Queries;

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
