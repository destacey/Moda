using System.Linq.Expressions;
using Moda.ProjectPortfolioManagement.Application.Projects.Models;
using Moda.ProjectPortfolioManagement.Application.ProjectTasks.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.ProjectPortfolioManagement.Domain.Services;

namespace Moda.ProjectPortfolioManagement.Application.ProjectTasks.Queries;

public sealed record GetProjectTaskTreeQuery : IQuery<IReadOnlyList<ProjectTaskTreeDto>>
{
    public GetProjectTaskTreeQuery(ProjectIdOrKey projectIdOrKey)
    {
        ProjectIdOrKeyFilter = projectIdOrKey.CreateProjectFilter<ProjectTask>();
    }

    public Expression<Func<ProjectTask, bool>> ProjectIdOrKeyFilter { get; }
}

internal sealed class GetProjectTaskTreeQueryHandler(IProjectPortfolioManagementDbContext ppmDbContext)
    : IQueryHandler<GetProjectTaskTreeQuery, IReadOnlyList<ProjectTaskTreeDto>>
{
    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;

    public async Task<IReadOnlyList<ProjectTaskTreeDto>> Handle(GetProjectTaskTreeQuery request, CancellationToken cancellationToken)
    {
        // Load all tasks for the project
        var tasks = await _ppmDbContext.ProjectTasks
            .Where(request.ProjectIdOrKeyFilter)
            .Include(t => t.Parent)
            .Include(t => t.Roles)
                .ThenInclude(a => a.Employee)
            .ToListAsync(cancellationToken);

        if (tasks.Count == 0)
            return [];

        // Calculate WBS codes for all tasks
        var wbsCodes = WbsCalculator.CalculateAllWbs(tasks);

        // Map to DTOs
        var dtos = tasks.Adapt<List<ProjectTaskTreeDto>>();

        // Assign WBS codes
        foreach (var dto in dtos)
        {
            if (wbsCodes.TryGetValue(dto.Id, out var wbs))
            {
                dto.Wbs = wbs;
            }
        }

        // Build hierarchy - filter to only root tasks
        var rootTasks = dtos.Where(t => t.ParentId is null).ToList();

        // Build children for each task
        BuildTaskHierarchy(rootTasks, dtos);

        return rootTasks;
    }

    private static void BuildTaskHierarchy(IEnumerable<ProjectTaskTreeDto> parentTasks, List<ProjectTaskTreeDto> allTasks)
    {
        foreach (var parent in parentTasks)
        {
            parent.Children = [.. allTasks.Where(t => t.ParentId == parent.Id)];
            if (parent.Children.Count != 0)
            {
                BuildTaskHierarchy(parent.Children, allTasks);
            }
        }
    }
}
