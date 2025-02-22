using Moda.ProjectPortfolioManagement.Application.Projects.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;

namespace Moda.ProjectPortfolioManagement.Application.Projects.Queries;

public sealed record GetProjectsQuery(ProjectStatus? StatusFilter) : IQuery<List<ProjectListDto>>;

internal sealed class GetProjectsQueryHandler(IProjectPortfolioManagementDbContext projectPortfolioManagementDbContext) 
    : IQueryHandler<GetProjectsQuery, List<ProjectListDto>>
{
    private readonly IProjectPortfolioManagementDbContext _projectPortfolioManagementDbContext = projectPortfolioManagementDbContext;

    public async Task<List<ProjectListDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var query = _projectPortfolioManagementDbContext.Projects.AsQueryable();

        if (request.StatusFilter.HasValue)
        {
            query = query.Where(pp => pp.Status == request.StatusFilter.Value);
        }

        return await query.ProjectToType<ProjectListDto>().ToListAsync(cancellationToken);
    }
}
