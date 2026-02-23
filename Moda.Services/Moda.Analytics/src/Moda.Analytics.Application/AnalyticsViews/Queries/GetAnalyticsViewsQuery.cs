using Moda.Analytics.Application.AnalyticsViews.Dtos;
using Moda.Analytics.Application.Persistence;

namespace Moda.Analytics.Application.AnalyticsViews.Queries;

public sealed record GetAnalyticsViewsQuery(bool IncludeInactive = false) : IQuery<IReadOnlyList<AnalyticsViewListDto>>;

internal sealed class GetAnalyticsViewsQueryHandler(
    IAnalyticsDbContext analyticsDbContext,
    ICurrentUser currentUser) : IQueryHandler<GetAnalyticsViewsQuery, IReadOnlyList<AnalyticsViewListDto>>
{
    private readonly IAnalyticsDbContext _analyticsDbContext = analyticsDbContext;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<IReadOnlyList<AnalyticsViewListDto>> Handle(GetAnalyticsViewsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.GetUserId();

        var query = _analyticsDbContext.AnalyticsViews
            .AsNoTracking()
            .Where(v => v.Visibility == Visibility.Public || v.OwnerId == currentUserId)
            .AsQueryable();

        if (!request.IncludeInactive)
        {
            query = query.Where(v => v.IsActive);
        }

        return await query
            .OrderBy(v => v.Name)
            .ProjectToType<AnalyticsViewListDto>()
            .ToListAsync(cancellationToken);
    }
}
