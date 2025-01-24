using Moda.StrategicManagement.Application.StrategicThemes.Dtos;
using Moda.StrategicManagement.Domain.Enums;

namespace Moda.StrategicManagement.Application.StrategicThemes.Queries;

/// <summary>
/// Retrieves a list of StrategicThemes based on the provided filter.  Returns all StrategicThemes if no filter is provided.
/// </summary>
/// <param name="StateFilter"></param>
public sealed record GetStrategicThemesQuery(StrategicThemeState? StateFilter) : IQuery<List<StrategicThemeListDto>>;

internal sealed class GetStrategicThemesQueryHandler(IStrategicManagementDbContext strategicManagementDbContext) : IQueryHandler<GetStrategicThemesQuery, List<StrategicThemeListDto>>
{
    private readonly IStrategicManagementDbContext _strategicManagementDbContext = strategicManagementDbContext;

    public async Task<List<StrategicThemeListDto>> Handle(GetStrategicThemesQuery request, CancellationToken cancellationToken)
    {
        var query = _strategicManagementDbContext.StrategicThemes.AsQueryable();

        if (request.StateFilter.HasValue)
        {
            query = query.Where(st => st.State == request.StateFilter.Value);
        }

        return await query.ProjectToType<StrategicThemeListDto>().ToListAsync(cancellationToken);
    }
}
