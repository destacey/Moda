using Moda.ProjectPortfolioManagement.Application.Projects.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.Projects.Queries;

public sealed record GetProjectPhasesQuery(Guid ProjectId) : IQuery<List<ProjectPhaseListDto>>;

internal sealed class GetProjectPhasesQueryHandler(IProjectPortfolioManagementDbContext ppmDbContext)
    : IQueryHandler<GetProjectPhasesQuery, List<ProjectPhaseListDto>>
{
    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;

    public async Task<List<ProjectPhaseListDto>> Handle(GetProjectPhasesQuery request, CancellationToken cancellationToken)
    {
        return await _ppmDbContext.ProjectPhases
            .Where(p => p.ProjectId == request.ProjectId)
            .OrderBy(p => p.Order)
            .ProjectToType<ProjectPhaseListDto>()
            .ToListAsync(cancellationToken);
    }
}
