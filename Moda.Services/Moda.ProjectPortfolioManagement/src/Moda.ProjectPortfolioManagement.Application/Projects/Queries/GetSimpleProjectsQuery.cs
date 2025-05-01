using Moda.Common.Domain.Interfaces.ProjectPortfolioManagement;

namespace Moda.ProjectPortfolioManagement.Application.Projects.Queries;

public sealed record GetSimpleProjectsQuery() : IQuery<List<ISimpleProject>>;

internal sealed class GetSimpleProjectsQueryHandler(IProjectPortfolioManagementDbContext projectPortfolioManagementDbContext) 
    : IQueryHandler<GetSimpleProjectsQuery, List<ISimpleProject>>
{
    private readonly IProjectPortfolioManagementDbContext _projectPortfolioManagementDbContext = projectPortfolioManagementDbContext;

    public async Task<List<ISimpleProject>> Handle(GetSimpleProjectsQuery request, CancellationToken cancellationToken)
    {
        var projects = await _projectPortfolioManagementDbContext.Projects
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return [.. projects.OfType<ISimpleProject>()];
    }
}
