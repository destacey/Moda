using Moda.Common.Application.Models;
using Moda.ProjectPortfolioManagement.Application.Portfolios.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Models;
using System.Linq.Expressions;

namespace Moda.ProjectPortfolioManagement.Application.Portfolios.Queries;
public sealed record GetProjectPortfolioQuery : IQuery<ProjectPortfolioDetailsDto?>
{
    public GetProjectPortfolioQuery(IdOrKey idOrKey)
    {
        IdOrKeyFilter = idOrKey.CreateFilter<ProjectPortfolio>();
    }

    public Expression<Func<ProjectPortfolio, bool>> IdOrKeyFilter { get; }
}

internal sealed class GetProjectPortfolioQueryHandler(IProjectPortfolioManagementDbContext projectPortfolioManagementDbContext) 
    : IQueryHandler<GetProjectPortfolioQuery, ProjectPortfolioDetailsDto?>
{
    private readonly IProjectPortfolioManagementDbContext _projectPortfolioManagementDbContext = projectPortfolioManagementDbContext;

    public async Task<ProjectPortfolioDetailsDto?> Handle(GetProjectPortfolioQuery request, CancellationToken cancellationToken)
    {
        return await _projectPortfolioManagementDbContext.Portfolios
            .Where(request.IdOrKeyFilter)
            .ProjectToType<ProjectPortfolioDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
