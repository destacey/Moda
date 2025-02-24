using Moda.Common.Domain.Enums.StrategicManagement;
using Moda.ProjectPortfolioManagement.Application.StrategicThemes.Dtos;

namespace Moda.ProjectPortfolioManagement.Application.StrategicThemes.Queries;

/// <summary>
/// Retrieves a list of StrategicThemes based on the provided filter.  Returns all StrategicThemes if no filter is provided.
/// </summary>
/// <param name="StateFilter"></param>
public sealed record GetStrategicThemesQuery(StrategicThemeState? StateFilter) : IQuery<List<PpmStrategicThemeListDto>>;

internal sealed class GetStrategicThemesQueryHandler(IProjectPortfolioManagementDbContext projectPortfolioManagementDbContext) : IQueryHandler<GetStrategicThemesQuery, List<PpmStrategicThemeListDto>>
{
    private readonly IProjectPortfolioManagementDbContext _projectPortfolioManagementDbContext = projectPortfolioManagementDbContext;

    public async Task<List<PpmStrategicThemeListDto>> Handle(GetStrategicThemesQuery request, CancellationToken cancellationToken)
    {
        var query = _projectPortfolioManagementDbContext.PpmStrategicThemes.AsQueryable();

        if (request.StateFilter.HasValue)
        {
            query = query.Where(st => st.State == request.StateFilter.Value);
        }

        return await query.ProjectToType<PpmStrategicThemeListDto>().ToListAsync(cancellationToken);
    }
}
