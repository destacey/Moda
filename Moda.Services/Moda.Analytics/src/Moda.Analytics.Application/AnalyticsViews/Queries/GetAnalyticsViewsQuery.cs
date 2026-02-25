using Ardalis.GuardClauses;
using Moda.Analytics.Application.AnalyticsViews.Dtos;
using Moda.Analytics.Application.Persistence;

namespace Moda.Analytics.Application.AnalyticsViews.Queries;

public sealed record GetAnalyticsViewsQuery(bool IncludeInactive = false) : IQuery<IReadOnlyList<AnalyticsViewListDto>>;

internal sealed class GetAnalyticsViewsQueryHandler(
    IAnalyticsDbContext analyticsDbContext,
    ICurrentUser currentUser) : IQueryHandler<GetAnalyticsViewsQuery, IReadOnlyList<AnalyticsViewListDto>>
{
    private readonly IAnalyticsDbContext _analyticsDbContext = analyticsDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());

    public async Task<IReadOnlyList<AnalyticsViewListDto>> Handle(GetAnalyticsViewsQuery request, CancellationToken cancellationToken)
    {
        var query = _analyticsDbContext.AnalyticsViews
            .AsNoTracking()
            .Where(v => v.Visibility == Visibility.Public || v.AnalyticsViewManagers.Any(m => m.ManagerId == _currentUserEmployeeId))
            .AsQueryable();

        if (!request.IncludeInactive)
        {
            query = query.Where(v => v.IsActive);
        }

        return await query
            .OrderBy(v => v.Name)
            .Select(v => new AnalyticsViewListDto
            {
                Id = v.Id,
                Name = v.Name,
                Description = v.Description,
                Dataset = v.Dataset,
                Visibility = v.Visibility,
                ManagerIds = v.AnalyticsViewManagers.Select(m => m.ManagerId).ToList(),
                IsActive = v.IsActive,
                LastModified = EF.Property<Instant>(v, "SystemLastModified")
            })
            .ToListAsync(cancellationToken);
    }
}
