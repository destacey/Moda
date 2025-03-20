using Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;

namespace Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Queries;

/// <summary>
/// Retrieves a list of strategic initiatives based on the provided filter.  Returns all strategic initiatives if no filter is provided.
/// </summary>
/// <param name="StatusFilter"></param>
public sealed record GetStrategicInitiativesQuery(StrategicInitiativeStatus? StatusFilter) : IQuery<List<StrategicInitiativeListDto>>;

internal sealed class GetStrategicInitiativesQueryHandler(IProjectPortfolioManagementDbContext projectPortfolioManagementDbContext)
    : IQueryHandler<GetStrategicInitiativesQuery, List<StrategicInitiativeListDto>>
{
    private readonly IProjectPortfolioManagementDbContext _projectPortfolioManagementDbContext = projectPortfolioManagementDbContext;

    public async Task<List<StrategicInitiativeListDto>> Handle(GetStrategicInitiativesQuery request, CancellationToken cancellationToken)
    {
        var query = _projectPortfolioManagementDbContext.StrategicInitiatives.AsQueryable();

        if (request.StatusFilter.HasValue)
        {
            query = query.Where(si => si.Status == request.StatusFilter.Value);
        }

        return await query.ProjectToType<StrategicInitiativeListDto>().ToListAsync(cancellationToken);
    }
}
