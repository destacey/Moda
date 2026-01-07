using System.Linq.Expressions;
using Moda.ProjectPortfolioManagement.Application.ProjectTasks.Dtos;
using Moda.ProjectPortfolioManagement.Application.ProjectTasks.Models;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.ProjectTasks.Queries;

public sealed record GetProjectTaskQuery : IQuery<ProjectTaskDto?>
{
    public GetProjectTaskQuery(ProjectTaskIdOrKey idOrTaskKey)
    {
        IdOrKeyFilter = idOrTaskKey.CreateFilter<ProjectTask>();
    }

    public Expression<Func<ProjectTask, bool>> IdOrKeyFilter { get; }
}

internal sealed class GetProjectTaskQueryHandler(IProjectPortfolioManagementDbContext ppmDbContext)
    : IQueryHandler<GetProjectTaskQuery, ProjectTaskDto?>
{
    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;

    public async Task<ProjectTaskDto?> Handle(GetProjectTaskQuery request, CancellationToken cancellationToken)
    {
        return await _ppmDbContext.ProjectTasks
            .Where(request.IdOrKeyFilter)
            .ProjectToType<ProjectTaskDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
