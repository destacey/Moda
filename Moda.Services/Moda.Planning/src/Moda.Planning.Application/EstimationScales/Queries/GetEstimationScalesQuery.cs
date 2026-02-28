using Moda.Planning.Application.EstimationScales.Dtos;

namespace Moda.Planning.Application.EstimationScales.Queries;

public sealed record GetEstimationScalesQuery() : IQuery<IReadOnlyList<EstimationScaleListDto>>;

internal sealed class GetEstimationScalesQueryHandler(IPlanningDbContext planningDbContext) : IQueryHandler<GetEstimationScalesQuery, IReadOnlyList<EstimationScaleListDto>>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;

    public async Task<IReadOnlyList<EstimationScaleListDto>> Handle(GetEstimationScalesQuery request, CancellationToken cancellationToken)
    {
        return await _planningDbContext.EstimationScales
            .ProjectToType<EstimationScaleListDto>()
            .ToListAsync(cancellationToken);
    }
}
