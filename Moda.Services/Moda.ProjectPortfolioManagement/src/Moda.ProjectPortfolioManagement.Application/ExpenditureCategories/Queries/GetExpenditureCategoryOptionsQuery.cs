using Moda.ProjectPortfolioManagement.Application.ExpenditureCategories.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;

namespace Moda.ProjectPortfolioManagement.Application.ExpenditureCategories.Queries;

public sealed record GetExpenditureCategoryOptionsQuery(bool? IncludeArchived) : IQuery<List<ExpenditureCategoryOptionDto>>;

internal sealed class GetExpenditureCategoryOptionsQueryHandler(IProjectPortfolioManagementDbContext projectPortfolioManagementDbContext)
    : IQueryHandler<GetExpenditureCategoryOptionsQuery, List<ExpenditureCategoryOptionDto>>
{
    private readonly IProjectPortfolioManagementDbContext _projectPortfolioManagementDbContext = projectPortfolioManagementDbContext;

    public async Task<List<ExpenditureCategoryOptionDto>> Handle(GetExpenditureCategoryOptionsQuery request, CancellationToken cancellationToken)
    {
        List<ExpenditureCategoryState> statusFilter = request.IncludeArchived ?? false
            ? [ExpenditureCategoryState.Active, ExpenditureCategoryState.Archived]
            : [ExpenditureCategoryState.Active];

        var categories = await _projectPortfolioManagementDbContext.ExpenditureCategories
            .Where(c => statusFilter.Contains(c.State))
            .ProjectToType<ExpenditureCategoryOptionDto>()
            .ToListAsync(cancellationToken);

        return [.. categories.OrderBy(c => c.Name)];
    }
}
