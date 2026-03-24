using Microsoft.EntityFrameworkCore;
using Moda.Common.Application.Interfaces;
using Moda.Common.Application.Search;
using Moda.Common.Application.Search.Dtos;
using Moda.Common.Extensions;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;

namespace Moda.ProjectPortfolioManagement.Application.Search;

internal sealed class SearchPpmForGlobalSearchQueryHandler(IProjectPortfolioManagementDbContext ppmDbContext)
    : IQueryHandler<SearchPpmForGlobalSearchQuery, ServiceSearchResponse>
{
    public async Task<ServiceSearchResponse> Handle(SearchPpmForGlobalSearchQuery request, CancellationToken cancellationToken)
    {
        var term = request.Request.SearchTerm;
        var max = request.Request.MaxResultsPerCategory;
        var categories = new List<GlobalSearchCategoryDto>();

        // Projects
        var projectQuery = ppmDbContext.Projects
            .Where(p => p.Name.Contains(term) || ((string)p.Key).Contains(term));

        var projectCount = await projectQuery.CountAsync(cancellationToken);
        var projectData = await projectQuery
            .Select(p => new { p.Name, Key = (string)p.Key, p.Status })
            .Take(max)
            .ToListAsync(cancellationToken);

        categories.Add(new GlobalSearchCategoryDto
        {
            Name = "Projects",
            Slug = "projects",
            Items = projectData.Select(p => new GlobalSearchResultItemDto
            {
                Title = p.Name,
                Subtitle = p.Status.GetDisplayName(),
                Key = p.Key,
                EntityType = nameof(Project)
            }).ToList(),
            TotalCount = projectCount
        });

        // Programs
        var programQuery = ppmDbContext.Programs
            .Where(p => p.Name.Contains(term));

        var programCount = await programQuery.CountAsync(cancellationToken);
        var programData = await programQuery
            .Select(p => new { p.Name, p.Key, p.Status })
            .Take(max)
            .ToListAsync(cancellationToken);

        categories.Add(new GlobalSearchCategoryDto
        {
            Name = "Programs",
            Slug = "programs",
            Items = programData.Select(p => new GlobalSearchResultItemDto
            {
                Title = p.Name,
                Subtitle = p.Status.GetDisplayName(),
                Key = p.Key.ToString(),
                EntityType = nameof(Moda.ProjectPortfolioManagement.Domain.Models.Program)
            }).ToList(),
            TotalCount = programCount
        });

        // Portfolios
        var portfolioQuery = ppmDbContext.Portfolios
            .Where(p => p.Name.Contains(term));

        var portfolioCount = await portfolioQuery.CountAsync(cancellationToken);
        var portfolioData = await portfolioQuery
            .Select(p => new { p.Name, p.Key, p.Status })
            .Take(max)
            .ToListAsync(cancellationToken);

        categories.Add(new GlobalSearchCategoryDto
        {
            Name = "Portfolios",
            Slug = "portfolios",
            Items = portfolioData.Select(p => new GlobalSearchResultItemDto
            {
                Title = p.Name,
                Subtitle = p.Status.GetDisplayName(),
                Key = p.Key.ToString(),
                EntityType = nameof(ProjectPortfolio)
            }).ToList(),
            TotalCount = portfolioCount
        });

        // Strategic Initiatives
        var siQuery = ppmDbContext.StrategicInitiatives
            .Where(si => si.Name.Contains(term));

        var siCount = await siQuery.CountAsync(cancellationToken);
        var siData = await siQuery
            .Select(si => new { si.Name, si.Key, si.Status })
            .Take(max)
            .ToListAsync(cancellationToken);

        categories.Add(new GlobalSearchCategoryDto
        {
            Name = "Strategic Initiatives",
            Slug = "strategic-initiatives",
            Items = siData.Select(si => new GlobalSearchResultItemDto
            {
                Title = si.Name,
                Subtitle = si.Status.GetDisplayName(),
                Key = si.Key.ToString(),
                EntityType = nameof(StrategicInitiative)
            }).ToList(),
            TotalCount = siCount
        });

        return new ServiceSearchResponse { Categories = categories };
    }
}
