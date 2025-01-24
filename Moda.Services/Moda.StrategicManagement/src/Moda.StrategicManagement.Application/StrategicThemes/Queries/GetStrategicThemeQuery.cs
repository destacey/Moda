using System.Linq.Expressions;
using Moda.Common.Application.Models;
using Moda.StrategicManagement.Application.StrategicThemes.Dtos;
using Moda.StrategicManagement.Application.Strategies.Dtos;
using Moda.StrategicManagement.Domain.Models;

namespace Moda.StrategicManagement.Application.StrategicThemes.Queries;

/// <summary>
/// Get a StrategicTheme by Id or Key
/// </summary>
public sealed record GetStrategicThemeQuery : IQuery<StrategicThemeDetailsDto?>
{
    public GetStrategicThemeQuery(IdOrKey idOrKey)
    {
        IdOrKeyFilter = idOrKey.CreateFilter<StrategicTheme>();
    }

    public Expression<Func<StrategicTheme, bool>> IdOrKeyFilter { get; }
}

internal sealed class GetStrategicThemeQueryHandler(IStrategicManagementDbContext strategicManagementDbContext) : IQueryHandler<GetStrategicThemeQuery, StrategicThemeDetailsDto?>
{
    private readonly IStrategicManagementDbContext _strategicManagementDbContext = strategicManagementDbContext;

    public async Task<StrategicThemeDetailsDto?> Handle(GetStrategicThemeQuery request, CancellationToken cancellationToken)
    {
        return await _strategicManagementDbContext.StrategicThemes
            .Where(request.IdOrKeyFilter)
            .ProjectToType<StrategicThemeDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
