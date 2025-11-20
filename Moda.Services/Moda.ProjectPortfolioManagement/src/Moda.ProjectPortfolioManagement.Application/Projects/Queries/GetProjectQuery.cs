using System.Linq.Expressions;
using Moda.Common.Application.Models;
using Moda.ProjectPortfolioManagement.Application.Projects.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.Projects.Queries;
public sealed record GetProjectQuery : IQuery<ProjectDetailsDto?>
{
    public GetProjectQuery(IdOrKey idOrKey)
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
        return await _ppmDbContext.Projects
            .Where(request.IdOrKeyFilter)
            .ProjectToType<ProjectDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
