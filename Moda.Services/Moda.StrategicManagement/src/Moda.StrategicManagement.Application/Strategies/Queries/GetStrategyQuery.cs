using System.Linq.Expressions;
using Moda.Common.Application.Models;
using Moda.StrategicManagement.Application.Strategies.Dtos;
using Moda.StrategicManagement.Domain.Models;

namespace Moda.StrategicManagement.Application.Strategies.Queries;

/// <summary>
/// Get a Strategy by Id or Key
/// </summary>
public sealed record GetStrategyQuery : IQuery<StrategyDetailsDto?>
{
    public GetStrategyQuery(IdOrKey idOrKey)
    {
        IdOrKeyFilter = idOrKey.CreateFilter<Strategy>();
    }

    public Expression<Func<Strategy, bool>> IdOrKeyFilter { get; }
}

internal sealed class GetStrategyQueryHandler(IStrategicManagementDbContext strategicManagementDbContext) : IQueryHandler<GetStrategyQuery, StrategyDetailsDto?>
{
    private readonly IStrategicManagementDbContext _strategicManagementDbContext = strategicManagementDbContext;

    public async Task<StrategyDetailsDto?> Handle(GetStrategyQuery request, CancellationToken cancellationToken)
    {
        return await _strategicManagementDbContext.Strategies
            .Where(request.IdOrKeyFilter)
            .ProjectToType<StrategyDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
