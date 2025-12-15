using System.Linq.Expressions;
using Moda.Common.Application.Models;
using Moda.ProjectPortfolioManagement.Application.ProjectTasks.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.ProjectTasks.Queries;

public sealed record GetProjectTaskQuery : IQuery<ProjectTaskDto?>
{
    public GetProjectTaskQuery(IdOrTaskKey idOrTaskKey)
    {
        IdOrTaskKeyFilter = idOrTaskKey.CreateTaskFilter<ProjectTask>(
            t => t.Id,
            t => t.TaskKey.Value
        );
    }

    public Expression<Func<ProjectTask, bool>> IdOrTaskKeyFilter { get; }
}

internal sealed class GetProjectTaskQueryHandler(IProjectPortfolioManagementDbContext ppmDbContext)
    : IQueryHandler<GetProjectTaskQuery, ProjectTaskDto?>
{
    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;

    public async Task<ProjectTaskDto?> Handle(GetProjectTaskQuery request, CancellationToken cancellationToken)
    {
        return await _ppmDbContext.ProjectTasks
            .Where(request.IdOrTaskKeyFilter)
            .ProjectToType<ProjectTaskDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
