using Moda.Planning.Application.EstimationScales.Dtos;

namespace Moda.Planning.Application.EstimationScales.Queries;

public sealed record GetEstimationScaleQuery(int Id) : IQuery<EstimationScaleDetailsDto?>;

internal sealed class GetEstimationScaleQueryHandler(IPlanningDbContext planningDbContext) : IQueryHandler<GetEstimationScaleQuery, EstimationScaleDetailsDto?>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;

    public async Task<EstimationScaleDetailsDto?> Handle(GetEstimationScaleQuery request, CancellationToken cancellationToken)
    {
        return await _planningDbContext.EstimationScales
            .Where(s => s.Id == request.Id)
            .ProjectToType<EstimationScaleDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
