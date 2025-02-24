using System.Linq.Expressions;
using Moda.Common.Application.Models;
using Moda.ProjectPortfolioManagement.Application.StrategicThemes.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.StrategicThemes.Queries;

/// <summary>
/// Get a StrategicTheme by Id or Key
/// </summary>
public sealed record GetStrategicThemeQuery : IQuery<PpmStrategicThemeDetailsDto?>
{
    public GetStrategicThemeQuery(IdOrKey idOrKey)
    {
        IdOrKeyFilter = idOrKey.CreateFilter<StrategicTheme>();
    }

    public Expression<Func<StrategicTheme, bool>> IdOrKeyFilter { get; }
}

internal sealed class GetStrategicThemeQueryHandler(IProjectPortfolioManagementDbContext projectPortfolioManagementDbContext) : IQueryHandler<GetStrategicThemeQuery, PpmStrategicThemeDetailsDto?>
{
    private readonly IProjectPortfolioManagementDbContext _projectPortfolioManagementDbContext = projectPortfolioManagementDbContext;

    public async Task<PpmStrategicThemeDetailsDto?> Handle(GetStrategicThemeQuery request, CancellationToken cancellationToken)
    {
        return await _projectPortfolioManagementDbContext.PpmStrategicThemes
            .Where(request.IdOrKeyFilter)
            .ProjectToType<PpmStrategicThemeDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
