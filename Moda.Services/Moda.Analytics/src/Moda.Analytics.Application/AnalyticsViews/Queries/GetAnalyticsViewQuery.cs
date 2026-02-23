using Moda.Analytics.Application.AnalyticsViews.Dtos;
using Moda.Analytics.Application.Persistence;

namespace Moda.Analytics.Application.AnalyticsViews.Queries;

public sealed record GetAnalyticsViewQuery(Guid Id) : IQuery<Result<AnalyticsViewDetailsDto>>;

internal sealed class GetAnalyticsViewQueryHandler(
    IAnalyticsDbContext analyticsDbContext,
    ICurrentUser currentUser) : IQueryHandler<GetAnalyticsViewQuery, Result<AnalyticsViewDetailsDto>>
{
    private readonly IAnalyticsDbContext _analyticsDbContext = analyticsDbContext;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<AnalyticsViewDetailsDto>> Handle(GetAnalyticsViewQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.GetUserId();

        var view = await _analyticsDbContext.AnalyticsViews
            .AsNoTracking()
            .Where(v => v.Id == request.Id)
            .Where(v => v.Visibility == Visibility.Public || v.OwnerId == currentUserId)
            .ProjectToType<AnalyticsViewDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);

        return view is null
            ? Result.Failure<AnalyticsViewDetailsDto>("Analytics view not found.")
            : Result.Success(view);
    }
}
