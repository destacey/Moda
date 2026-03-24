using Moda.Common.Application.Search;
using Moda.Common.Application.Search.Dtos;
using Moda.Common.Domain.Employees;
using Moda.Common.Domain.Enums.Organization;

namespace Moda.Organization.Application.Search;

internal sealed class SearchOrganizationForGlobalSearchQueryHandler(IOrganizationDbContext organizationDbContext)
    : IQueryHandler<SearchOrganizationForGlobalSearchQuery, ServiceSearchResponse>
{
    public async Task<ServiceSearchResponse> Handle(SearchOrganizationForGlobalSearchQuery request, CancellationToken cancellationToken)
    {
        var term = request.Request.SearchTerm;
        var max = request.Request.MaxResultsPerCategory;
        var categories = new List<GlobalSearchCategoryDto>();

        // Teams (includes Teams and TeamsOfTeams via BaseTeams)
        var teamQuery = organizationDbContext.BaseTeams
            .Where(t => !t.IsDeleted)
            .Where(t => t.Name.Contains(term) || ((string)t.Code).Contains(term));

        var teamCount = await teamQuery.CountAsync(cancellationToken);
        var teams = await teamQuery
            .Select(t => new GlobalSearchResultItemDto
            {
                Title = t.Name,
                Subtitle = t.IsActive ? null : "Inactive",
                Key = ((string)t.Code),
                EntityType = t.Type == TeamType.TeamOfTeams ? nameof(TeamOfTeams) : nameof(Team),
                AuxKey = t.Key.ToString()
            })
            .Take(max)
            .ToListAsync(cancellationToken);

        categories.Add(new GlobalSearchCategoryDto
        {
            Name = "Teams",
            Slug = "teams",
            Items = teams,
            TotalCount = teamCount
        });

        // Employees
        var employeeQuery = organizationDbContext.Employees
            .Where(e => !e.IsDeleted)
            .Where(e => e.Name.FirstName.Contains(term)
                || e.Name.LastName.Contains(term));

        var employeeCount = await employeeQuery.CountAsync(cancellationToken);
        var employees = await employeeQuery
            .Select(e => new GlobalSearchResultItemDto
            {
                Title = e.Name.FirstName + " " + e.Name.LastName,
                Subtitle = e.IsActive ? null : "Inactive",
                Key = e.Key.ToString(),
                EntityType = nameof(Employee)
            })
            .Take(max)
            .ToListAsync(cancellationToken);

        categories.Add(new GlobalSearchCategoryDto
        {
            Name = "Employees",
            Slug = "employees",
            Items = employees,
            TotalCount = employeeCount
        });

        return new ServiceSearchResponse { Categories = categories };
    }
}
