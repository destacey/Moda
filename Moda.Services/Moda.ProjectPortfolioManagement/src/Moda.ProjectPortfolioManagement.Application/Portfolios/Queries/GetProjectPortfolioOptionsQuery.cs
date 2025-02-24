﻿using Moda.ProjectPortfolioManagement.Application.Portfolios.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;

namespace Moda.ProjectPortfolioManagement.Application.Portfolios.Queries;

/// <summary>
/// Retrieves a list of ProjectPortfolios based on the provided filter.  Returns all ProjectPortfolios if no filter is provided.
/// </summary>
/// <param name="StatusFilter"></param>
public sealed record GetProjectPortfolioOptionsQuery() : IQuery<List<ProjectPortfolioOptionDto>>;

internal sealed class GetProjectPortfolioOptionsQueryHandler(IProjectPortfolioManagementDbContext projectPortfolioManagementDbContext) 
    : IQueryHandler<GetProjectPortfolioOptionsQuery, List<ProjectPortfolioOptionDto>>
{
    private readonly IProjectPortfolioManagementDbContext _projectPortfolioManagementDbContext = projectPortfolioManagementDbContext;

    public async Task<List<ProjectPortfolioOptionDto>> Handle(GetProjectPortfolioOptionsQuery request, CancellationToken cancellationToken)
    {
        var portfolios = await _projectPortfolioManagementDbContext.Portfolios
            .Where(p => p.Status == ProjectPortfolioStatus.Active)
            .ProjectToType<ProjectPortfolioOptionDto>()
            .ToListAsync(cancellationToken);

        return [.. portfolios.OrderBy(p => p.Name)];
    }
}
