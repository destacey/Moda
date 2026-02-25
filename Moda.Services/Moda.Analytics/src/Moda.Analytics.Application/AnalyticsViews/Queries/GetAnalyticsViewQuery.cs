using Ardalis.GuardClauses;
using Moda.Analytics.Application.AnalyticsViews.Dtos;
using Moda.Analytics.Application.Persistence;

namespace Moda.Analytics.Application.AnalyticsViews.Queries;

public sealed record GetAnalyticsViewQuery(Guid Id) : IQuery<Result<AnalyticsViewDetailsDto>>;

internal sealed class GetAnalyticsViewQueryHandler(
    IAnalyticsDbContext analyticsDbContext,
    ICurrentUser currentUser) : IQueryHandler<GetAnalyticsViewQuery, Result<AnalyticsViewDetailsDto>>
{
    private readonly IAnalyticsDbContext _analyticsDbContext = analyticsDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());

    public async Task<Result<AnalyticsViewDetailsDto>> Handle(GetAnalyticsViewQuery request, CancellationToken cancellationToken)
    {
        var view = await _analyticsDbContext.AnalyticsViews
            .AsNoTracking()
            .Where(v => v.Id == request.Id)
            .Where(v => v.Visibility == Visibility.Public || v.AnalyticsViewManagers.Any(m => m.ManagerId == _currentUserEmployeeId))
            .Select(v => new AnalyticsViewDetailsDto
            {
                Id = v.Id,
                Name = v.Name,
                Description = v.Description,
                Dataset = v.Dataset,
                DefinitionJson = v.DefinitionJson,
                Visibility = v.Visibility,
                ManagerIds = v.AnalyticsViewManagers.Select(m => m.ManagerId).ToList(),
                IsActive = v.IsActive,
                Created = EF.Property<Instant>(v, "SystemCreated"),
                LastModified = EF.Property<Instant>(v, "SystemLastModified")
            })
            .FirstOrDefaultAsync(cancellationToken);

        return view is null
            ? Result.Failure<AnalyticsViewDetailsDto>("Analytics view not found.")
            : Result.Success(view);
    }
}
