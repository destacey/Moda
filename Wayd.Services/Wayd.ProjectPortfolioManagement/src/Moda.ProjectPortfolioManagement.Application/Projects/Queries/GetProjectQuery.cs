using System.Linq.Expressions;
using Wayd.Common.Application.Models;
using Wayd.ProjectPortfolioManagement.Application.Projects.Dtos;
using Wayd.ProjectPortfolioManagement.Application.Projects.Models;
using Wayd.ProjectPortfolioManagement.Domain.Models;

namespace Wayd.ProjectPortfolioManagement.Application.Projects.Queries;

public sealed record GetProjectQuery : IQuery<ProjectDetailsDto?>
{
    public GetProjectQuery(ProjectIdOrKey idOrKey)
    {
        IdOrKeyFilter = idOrKey.CreateFilter<Project>();
    }

    public Expression<Func<Project, bool>> IdOrKeyFilter { get; }
}

internal sealed class GetProjectQueryHandler(IProjectPortfolioManagementDbContext ppmDbContext)
    : IQueryHandler<GetProjectQuery, ProjectDetailsDto?>
{
    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;

    public async Task<ProjectDetailsDto?> Handle(GetProjectQuery request, CancellationToken cancellationToken)
    {
        var project = await _ppmDbContext.Projects
            .Where(request.IdOrKeyFilter)
            .ProjectToType<ProjectDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);

        return project;
    }
}
