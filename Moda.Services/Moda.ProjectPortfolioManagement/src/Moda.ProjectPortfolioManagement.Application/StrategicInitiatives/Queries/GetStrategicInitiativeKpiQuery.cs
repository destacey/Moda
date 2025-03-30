using System.Linq.Expressions;
using Moda.Common.Application.Models;
using Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;

namespace Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Queries;

public sealed record GetStrategicInitiativeKpiQuery : IQuery<StrategicInitiativeKpiDetailsDto?>
{
    public GetStrategicInitiativeKpiQuery(IdOrKey strategicInitiativeIdOrKey, IdOrKey kpiIdOrKey)
    {
        StrategicInitiativeIdOrKeyFilter = strategicInitiativeIdOrKey.CreateFilter<StrategicInitiative>();
        KpiIdOrKey = kpiIdOrKey;
    }

    public Expression<Func<StrategicInitiative, bool>> StrategicInitiativeIdOrKeyFilter { get; }

    public IdOrKey KpiIdOrKey { get; }
}

internal sealed class GetStrategicInitiativeKpiQueryHandler(IProjectPortfolioManagementDbContext projectPortfolioManagementDbContext)
    : IQueryHandler<GetStrategicInitiativeKpiQuery, StrategicInitiativeKpiDetailsDto?>
{
    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = projectPortfolioManagementDbContext;

    public async Task<StrategicInitiativeKpiDetailsDto?> Handle(GetStrategicInitiativeKpiQuery request, CancellationToken cancellationToken)
    {
        var query = _ppmDbContext.StrategicInitiatives
            .Where(request.StrategicInitiativeIdOrKeyFilter);

        if (await query.AnyAsync(cancellationToken) == false)
        {
            return null;
        }

        var kpiQuery = GetKpiQuery(query, request.KpiIdOrKey);

        return await kpiQuery
                .ProjectToType<StrategicInitiativeKpiDetailsDto>()
                .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<StrategicInitiativeKpi> GetKpiQuery(IQueryable<StrategicInitiative> query, IdOrKey kpiIdOrKey)
    {
        return kpiIdOrKey.IsId
            ? query.SelectMany(i => i.Kpis.Where(k => k.Id == kpiIdOrKey.AsId))
            : query.SelectMany(i => i.Kpis.Where(k => k.Key == kpiIdOrKey.AsKey));
    }
}
