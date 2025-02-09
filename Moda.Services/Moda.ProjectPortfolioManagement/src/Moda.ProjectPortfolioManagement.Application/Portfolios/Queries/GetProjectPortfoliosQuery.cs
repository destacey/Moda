using Moda.ProjectPortfolioManagement.Application.Portfolios.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;

namespace Moda.ProjectPortfolioManagement.Application.Portfolios.Queries;

/// <summary>
/// Retrieves a list of ProjectPortfolios based on the provided filter.  Returns all ProjectPortfolios if no filter is provided.
/// </summary>
/// <param name="StatusFilter"></param>
public sealed record GetProjectPortfoliosQuery(ProjectPortfolioStatus? StatusFilter) : IQuery<List<ProjectPortfolioListDto>>;

internal sealed class GetProjectPortfoliosQueryHandler(IProjectPortfolioManagementDbContext projectPortfolioManagementDbContext) 
    : IQueryHandler<GetProjectPortfoliosQuery, List<ProjectPortfolioListDto>>
{
    private readonly IProjectPortfolioManagementDbContext _projectPortfolioManagementDbContext = projectPortfolioManagementDbContext;

    public async Task<List<ProjectPortfolioListDto>> Handle(GetProjectPortfoliosQuery request, CancellationToken cancellationToken)
    {
        var query = _projectPortfolioManagementDbContext.Portfolios.AsQueryable();

        if (request.StatusFilter.HasValue)
        {
            query = query.Where(pp => pp.Status == request.StatusFilter.Value);
        }

        return await query.ProjectToType<ProjectPortfolioListDto>().ToListAsync(cancellationToken);
    }
}
