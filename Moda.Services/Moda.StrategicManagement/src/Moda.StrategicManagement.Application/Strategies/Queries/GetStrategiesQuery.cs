using Moda.StrategicManagement.Application.Strategies.Dtos;
using Moda.StrategicManagement.Domain.Enums;

namespace Moda.StrategicManagement.Application.Strategies.Queries;

/// <summary>
/// Retrieves a list of Strategies based on the provided filter.  Returns all Strategies if no filter is provided.
/// </summary>
/// <param name="StatusFilter"></param>
public sealed record GetStrategiesQuery(StrategyStatus? StatusFilter) : IQuery<List<StrategyListDto>>;

internal sealed class GetStrategysQueryHandler(IStrategicManagementDbContext strategicManagementDbContext) : IQueryHandler<GetStrategiesQuery, List<StrategyListDto>>
{
    private readonly IStrategicManagementDbContext _strategicManagementDbContext = strategicManagementDbContext;

    public async Task<List<StrategyListDto>> Handle(GetStrategiesQuery request, CancellationToken cancellationToken)
    {
        var query = _strategicManagementDbContext.Strategies.AsQueryable();

        if (request.StatusFilter.HasValue)
        {
            query = query.Where(st => st.Status == request.StatusFilter.Value);
        }

        return await query.ProjectToType<StrategyListDto>().ToListAsync(cancellationToken);
    }
}
